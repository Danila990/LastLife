using System;
using System.Collections.Generic;
using Core.Boosts.Impl;
using UniRx;

namespace Core.Boosts.Inventory
{
	public interface IBoostsInventory
	{
		IReactiveCommand<StoredBoost> OnBoostAdded { get; }
		IReactiveCommand<StoredBoost> OnBoostRemoved { get; }
		IReadOnlyDictionary<string, StoredBoost> Boosts { get; }
		bool TryGet(string type, out StoredBoost boost);
	}
	
	public class BoostsInventory : IDisposable, IBoostsInventory
	{
		private readonly SortedList<string, StoredBoost> _boosts = new SortedList<string, StoredBoost>();

		private bool _isDisposed;
			
		private readonly ReactiveCommand<StoredBoost> _onBoostAdded = new ReactiveCommand<StoredBoost>();
		private readonly ReactiveCommand<StoredBoost> _onBoostRemoved = new ReactiveCommand<StoredBoost>();

		public IReactiveCommand<StoredBoost> OnBoostAdded => _onBoostAdded;
		public IReactiveCommand<StoredBoost> OnBoostRemoved => _onBoostRemoved;
		public IReadOnlyDictionary<string, StoredBoost> Boosts => _boosts;


		public void Add(BoostArgs args)
		{
			if(_isDisposed)
				return;
			
			if(string.IsNullOrEmpty(args.Type) || args.Duration <= 0)
				return;

			Add(args, 1);
		}

		public void Add(BoostArgs args, int quantity)
		{
			if(quantity <= 0)
				return;
			
			var storedBoost = new StoredBoost(args, quantity);
			if (!_boosts.TryAdd(args.Type, storedBoost))
			{
				var boost = _boosts[args.Type];
				boost.Quantity += quantity;
				_boosts[args.Type] = boost;
			}
			
			_onBoostAdded.Execute(_boosts[args.Type]);
		}

		public bool TryGet(string type, out StoredBoost boost)
		{
			return _boosts.TryGetValue(type, out boost);
		}

		public bool TryGetAndDecreaseQuantity(string type, out BoostArgs boost)
		{
			boost = default;
			
			if(_isDisposed)
				return false;
			if (!TryGet(type, out var storedBoost))
				return false;
			if (storedBoost.Quantity <= 0)
				return false;
			
			boost = storedBoost.Args;
			storedBoost.Quantity--;

			if (storedBoost.Quantity > 0)
				_boosts[storedBoost.Args.Type] = storedBoost;
			else
				_boosts.Remove(storedBoost.Args.Type);

			_onBoostRemoved.Execute(storedBoost);
			return true;

		}
		public void Dispose()
		{
			_isDisposed = true;
			_onBoostAdded?.Dispose();
			_onBoostRemoved?.Dispose();
		}
	}
	
	[Serializable]
	public struct StoredBoost
	{
		public BoostArgs Args;
		public int Quantity;

		public StoredBoost(BoostArgs args, int  quantity)
		{
			Args = args;
			Quantity = quantity;
		}
	}
}
