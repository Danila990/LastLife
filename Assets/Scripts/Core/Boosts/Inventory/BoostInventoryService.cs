using System;
using System.Collections.Generic;
using Core.Boosts.Impl;
using Core.InputSystem;
using Core.Services;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Boosts.Inventory
{
	public class BoostInventoryService : IPostInitializable, IDisposable
	{
		private readonly IBoostInventorySaveAdapter _boostSaveAdapter;
		private readonly IPlayerSpawnService _spawnService;
		private readonly IItemStorage _itemStorage;
		private IDisposable _disposable;

		public BoostInventoryService(
			IBoostInventorySaveAdapter boostSaveAdapter,
			IPlayerSpawnService spawnService,
			IItemStorage itemStorage)
		{
			_boostSaveAdapter = boostSaveAdapter;
			_spawnService = spawnService;
			_itemStorage = itemStorage;
		}
		
		public void PostInitialize()
		{
			_disposable = _boostSaveAdapter.OnRestore.Subscribe(OnRestore);
		}

		private void OnRestore(IReadOnlyCollection<BoostInventorySaveAdapter.BoostSave> savedData)
		{
			foreach (var boost in savedData)
			{
				if (string.IsNullOrEmpty(boost.BoostObjectId))
					continue;	
				if (_itemStorage.BoostsByBoostType.TryGetValue(boost.BoostObjectId, out var boostObject))
				{
					_spawnService.PlayerCharacterAdapter.BoostsInventory.Add(boostObject.BoostArgs, boost.Quantity);
				}
				else
				{
					Debug.LogWarning("Can't find boost with id: " + boost.BoostObjectId);
				}
			}
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}
