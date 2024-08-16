using System;
using System.Collections.Generic;
using System.Linq;
using Core.Boosts.Builder;
using Core.Boosts.Entity;
using Core.Boosts.Inventory;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Factory.Inventory;
using Core.Services;
using Db.ObjectData;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Boosts.Impl
{
	public interface IBoostProvider : IDisposable
	{
		public ReactiveCommand<AppliedBoostArgs> OnBoostApplied { get; }
		public ReactiveCommand<BoostArgs> OnBoostEnded { get; }

		public bool GetBoostValue(string type, out float value);
		public void ApplyBoost(string args);
		public void ApplyBoostForce(in BoostArgs boost);
		
		public IReadOnlyDictionary<string, CompositeBoostHandler> ActiveBoosts { get; }
		
		IBoostsInventory BoostsInventory { get; }
	}
	
	public class MechBoostProvider : IBoostProvider
	{
		public MechBoostProvider()
		{
			OnBoostApplied = new ReactiveCommand<AppliedBoostArgs>();
			OnBoostEnded = new ReactiveCommand<BoostArgs>();
		}
		
		public void Dispose()
		{
		}

		public ReactiveCommand<AppliedBoostArgs> OnBoostApplied { get; }
		public ReactiveCommand<BoostArgs> OnBoostEnded { get; }
		public bool GetBoostValue(string type, out float value)
		{
			value = 0;
			return false;
		}

		public void ApplyBoost(string args)
		{
		}

		public void ApplyBoostForce(in BoostArgs boost)
		{
		}

		public IReadOnlyDictionary<string, CompositeBoostHandler> ActiveBoosts { get; }
		public IBoostsInventory BoostsInventory { get; }
	}

	public class PlayerBoostProvider : IBoostProvider
	{
		private readonly IObjectResolver _resolver;
		private readonly PlayerCharacterAdapter _adapter;
		private readonly BoostsInventory _inventory;
		private readonly Dictionary<string, CompositeBoostHandler> _activeBoosts = new Dictionary<string, CompositeBoostHandler>();
		private readonly Dictionary<string, AppliedBoost> _appliedBoosts = new Dictionary<string, AppliedBoost>();

		private readonly ReactiveCommand<AppliedBoostArgs> _onBoostApplied;
		private readonly ReactiveCommand<BoostArgs> _onBoostEnded;
		private readonly IDisposable _subDisposable;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly InventoryObjectData _syringeItem;

		private CharacterContext _characterContext;
		public IReadOnlyDictionary<string, CompositeBoostHandler> ActiveBoosts => _activeBoosts;
		public ReactiveCommand<AppliedBoostArgs> OnBoostApplied => _onBoostApplied;
		public ReactiveCommand<BoostArgs> OnBoostEnded => _onBoostEnded;
		public IBoostsInventory BoostsInventory => _inventory;

		public PlayerBoostProvider(IObjectResolver resolver, PlayerCharacterAdapter adapter, BoostsInventory inventory)
		{
			_resolver = resolver;
			_adapter = adapter;
			_inventory = inventory;
			_syringeItem = resolver.Resolve<IItemStorage>().InventoryItems["BoostInventoryItem"];
			
			_inventory.OnBoostAdded.Subscribe(OnBoostAdded).AddTo(_compositeDisposable);
			_inventory.OnBoostRemoved.Subscribe(OnBoostRemoved).AddTo(_compositeDisposable);
			_onBoostApplied = new ReactiveCommand<AppliedBoostArgs>().AddTo(_compositeDisposable);
			_onBoostEnded = new ReactiveCommand<BoostArgs>().AddTo(_compositeDisposable);
		}
		
		private void OnBoostRemoved(StoredBoost obj)
		{
			if (!_characterContext)
				return;
			
			if (_inventory.Boosts.Count > 0)
				return;
			
			_characterContext.Inventory.RemoveItem("BoostInventoryItem");
		}

		private void OnBoostAdded(StoredBoost obj)
		{
			if (!_characterContext)
				return;

			if (_characterContext.Inventory.InventoryItems.FirstOrDefault(pair => pair.ItemContext is BoostEntity).ItemContext is not BoostEntity haveAnyBoost)
			{ 
				_characterContext.Inventory.AddItem(_syringeItem, 1);
			}
			else
			{
				haveAnyBoost.UpdateView();
			}
		}

		public void OnContextChanged(CharacterContext characterContext)
		{
			_characterContext = characterContext;

			var haveAnyBoost = _characterContext.Inventory.InventoryItems.Any(pair => pair.ItemContext is BoostEntity);

			if (haveAnyBoost || _inventory.Boosts.Count == 0)
				return;
			
			_characterContext.Inventory.AddItem(_syringeItem, 1);
		}

		public void ApplyBoost(string boostType)
		{
			if (TryDecrease(boostType, out var args))
			{
				ApplyBoostInternal(args);
			}
		}
		
		public void ApplyBoostForce(in BoostArgs boost)
		{
			ApplyBoostInternal(boost, true);
		}
		
		private bool TryDecrease(string boostType, out BoostArgs args)
		{
			if (!_adapter.CurrentContext)
			{
				args = default;				
				return false;
			}

			if (!_inventory.TryGetAndDecreaseQuantity(boostType, out args))
			{
				Debug.LogWarning($"Cant decrease {boostType}");
				return false;
			}

			return true;
		}

		
		private void ApplyBoostInternal(BoostArgs args, bool suppress = false)
		{
			var type = args.Type;

			if (_appliedBoosts.TryGetValue(type, out var endHandler))
				endHandler.Dispose();

			var builder = new PlayerBoostBuilder(_resolver, _adapter.CurrentContext);

			var handler = builder.Create(args).ToCompositeBoostHandler();

			var sub = handler.BoostImpl.OnRemove.Subscribe(OnBoostEnd);
			_activeBoosts.Add(type, handler);
			_appliedBoosts.Add(type, new AppliedBoost(handler, sub));

			handler.BoostImpl.ApplyEffect();
			
			var eventArgs = new AppliedBoostArgs(args, suppress);
			_onBoostApplied.Execute(eventArgs);
		}


		public bool GetBoostValue(string type, out float value)
		{
			value = 0;
			if (_activeBoosts.TryGetValue(type, out var handler))
			{
				value = handler.Boost.BoostArgs.Value;
				return true;
			}

			return false;
		}
		
		private void OnBoostEnd(string type)
		{
			if (_activeBoosts.TryGetValue(type, out var boostHandler))
			{
				_onBoostEnded.Execute(boostHandler.BoostImpl.BoostArgs);
				_activeBoosts.Remove(type);
				_appliedBoosts.Remove(type);
			}
		}
		
		public void Dispose()
		{
			foreach (var boost in _appliedBoosts.Values)
				boost.Dispose();
			
			_compositeDisposable?.Dispose();
		}
		
		private readonly struct AppliedBoost : IDisposable
		{
			public readonly CompositeBoostHandler CompositeBoostHandler;
			public readonly IDisposable Sub;

			public AppliedBoost(CompositeBoostHandler compositeBoostHandler, IDisposable sub)
			{
				CompositeBoostHandler = compositeBoostHandler;
				Sub = sub;
			}
			
			public void Dispose()
			{
				CompositeBoostHandler?.Dispose();
				Sub?.Dispose();
			}
		}
	}
	
	public struct AppliedBoostArgs
	{
		public readonly BoostArgs BoostArgs;
		public readonly bool Suppressed;

		public AppliedBoostArgs(BoostArgs boostArgs, bool suppressed)
		{
			BoostArgs = boostArgs;
			Suppressed = suppressed;
		}
	}
}
