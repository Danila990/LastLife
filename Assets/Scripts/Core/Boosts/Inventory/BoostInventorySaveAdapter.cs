using System;
using System.Collections.Generic;
using System.Linq;
using Core.InputSystem;
using Core.Services.SaveSystem;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Boosts.Inventory
{
	public interface IBoostInventorySaveAdapter
	{
		public IReactiveCommand<IReadOnlyCollection<BoostInventorySaveAdapter.BoostSave>> OnRestore { get; }
	}
	
	public class BoostInventorySaveAdapter : IAutoLoadAdapter, IPostInitializable, IBoostInventorySaveAdapter
	{
		private readonly IPlayerSpawnService _spawnService;

		private BoostsInventory _boostsInventory;

		private readonly ReactiveCommand<IReadOnlyCollection<BoostSave>> _onRestore;

		public bool CanSave => true;
		public string SaveKey => "BoostsInventory";
		public IReactiveCommand<IReadOnlyCollection<BoostSave>> OnRestore => _onRestore;
		
		public BoostInventorySaveAdapter(IPlayerSpawnService spawnService)
		{
			_spawnService = spawnService;
			_boostsInventory = new BoostsInventory();
			_onRestore = new ReactiveCommand<IReadOnlyCollection<BoostSave>>();
			
			_boostsInventory.Boosts ??= new Dictionary<string, BoostSave>();
		}
		
		public void PostInitialize()
		{
			var adapter = _spawnService.PlayerCharacterAdapter;
			_spawnService.PlayerCharacterAdapter.BoostsInventory.OnBoostAdded.Subscribe(OnBoostAdded).AddTo(adapter);
			_spawnService.PlayerCharacterAdapter.BoostsInventory.OnBoostRemoved.Subscribe(OnBoostRemoved).AddTo(adapter);
		}
		
		public string CreateSave()
		{
			try
			{
				return JsonConvert.SerializeObject(_boostsInventory);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(BoostInventorySaveAdapter)}]" + e.Message);
			}

			return string.Empty;
		}
		
		public void LoadSave(string value)
		{
			try
			{
				_boostsInventory = JsonConvert.DeserializeObject<BoostsInventory>(value);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(BoostInventorySaveAdapter)}]" + e.Message);
			}
			
			_boostsInventory.Boosts ??= new Dictionary<string, BoostSave>();

			if(_boostsInventory.Boosts.Count > 0)
				_onRestore.Execute(_boostsInventory.Boosts.Values.ToArray());
		}

		private void OnBoostAdded(StoredBoost storedBoost)
		{
			_boostsInventory.Boosts ??= new();

			_boostsInventory.Boosts[storedBoost.Args.Type] = new BoostSave(storedBoost);
		}

		private void OnBoostRemoved(StoredBoost storedBoost)
		{
			if(_boostsInventory.Boosts == null)
				return;

			var key = storedBoost.Args.Type;

			if (!_boostsInventory.Boosts.ContainsKey(key))
				return;
			
			if (storedBoost.Quantity <= 0)
				_boostsInventory.Boosts.Remove(key);
			else
				_boostsInventory.Boosts[key] = new BoostSave(storedBoost);
		}

		[Serializable]
		private struct BoostsInventory
		{
			public Dictionary<string, BoostSave> Boosts;
		}

		[Serializable]
		public struct BoostSave
		{
			public string BoostObjectId;
			public int Quantity;

			public BoostSave(StoredBoost args)
			{
				BoostObjectId = args.Args.Type;
				Quantity = args.Quantity;
			}
		}
	}
}