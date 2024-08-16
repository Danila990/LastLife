using System;
using System.Linq;
using Core.Entity.Characters;
using Core.Equipment.Data;
using Core.Equipment.Inventory;
using Core.InputSystem;
using Core.Services;
using SharedUtils.PlayerPrefs.Impl;
using UniRx;
using VContainer.Unity;

namespace Core.Equipment
{

	public class EquipmentRecreateService : IPostInitializable, IDisposable
	{
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IItemStorage _itemStorage;
		private readonly IEquipmentInventoryService _equipmentInventoryService;
		private readonly IEquipmentSaveAdapter _equipmentSaveAdapter;
		private readonly InMemoryPlayerPrefsManager _memoryPrefs;
		private readonly ISceneLoaderService _sceneLoader;
		
		private Action _recreate;
		private CharacterContext _currentContext;
		private CompositeDisposable _disposable;
		private IDisposable _destroySub;

		private const string LIFE_SCOPE_PREFS_KEY = "lifetime_scope_equipment";

		public EquipmentRecreateService(
			IPlayerSpawnService playerSpawnService,
			IItemStorage itemStorage,
			IEquipmentInventoryService equipmentInventoryService,
			IEquipmentSaveAdapter equipmentSaveAdapter,
			InMemoryPlayerPrefsManager memoryPrefs,
			ISceneLoaderService sceneLoader
			)
		{
			_playerSpawnService = playerSpawnService;
			_itemStorage = itemStorage;
			_equipmentInventoryService = equipmentInventoryService;
			_equipmentSaveAdapter = equipmentSaveAdapter;
			_memoryPrefs = memoryPrefs;
			_sceneLoader = sceneLoader;
		}
		
		public void PostInitialize()
		{
			_recreate = () =>
			{
				RecreateCompletely();
				RecreateFromSave();
				RecreateFromCache();
				RecreateActiveEquipment();
			};
			_disposable?.Dispose();
			_disposable = new CompositeDisposable();
			_playerSpawnService.PlayerCharacterAdapter.ContextChanged.Subscribe(OnContextChanged).AddTo(_disposable);
			_sceneLoader.BeforeSceneChange.Subscribe(_ => OnBeforeSceneChange()).AddTo(_disposable);
		}

		private void OnContextChanged(CharacterContext context)
		{
			_currentContext = context;
			_destroySub?.Dispose();
			_destroySub = null;
			if(!context)
				return;
			
			_destroySub = _currentContext.OnDestroyCommand.Subscribe(OnContextDestroyed);
			
			_recreate?.Invoke();
			_recreate = null;
		}

		private void OnBeforeSceneChange()
		{
			CacheLifeTimeEquipment();
		}
		
		private void OnContextDestroyed(Unit _)
		{
			
			_destroySub?.Dispose();
			if (!_currentContext)
				return;

			if (_currentContext.Health.IsDeath)
			{
				_recreate = () =>
				{
					RecreateCompletely();
					RecreateFromSave();
					RecreateActiveEquipment();
				};
			}
			else
			{
				CacheLifeTimeEquipment();
				_recreate = () =>
				{
					RecreateCompletely();
					RecreateFromCache();
					RecreateActiveEquipment();
				};
			}
		}

		public void CacheLifeTimeEquipment()
		{
			if(!_currentContext)
				return;

			var cache = _currentContext.EquipmentInventory.Controller.AllEquipment.Items.Select(x => x).ToArray();
			_memoryPrefs.SetValue(LIFE_SCOPE_PREFS_KEY, cache);
		}
		
		private void RecreateFromSave()
		{
			if (!_equipmentSaveAdapter.TryGetEquipment(out var equipment))
				return;

			foreach (var item in equipment)
			{
				
				if (item.Type == EquipmentPartType.JetPack)
				{
					var key = (item.Type, item.ItemId);
					if (_currentContext.EquipmentInventory.Controller.AllEquipment.EquipmentByTypeId.TryGetValue(key, out var runtimeArgs))
					{
						var existedJetPackArgs = runtimeArgs.EquipmentArgs as JetPackItemArgs;
						existedJetPackArgs.Fuel = item.Fuel;
  						_equipmentInventoryService.AddExistedEquipment(runtimeArgs);
					}
				}
			}
		}
		
		private void RecreateCompletely()
		{
			var inventory = _currentContext.EquipmentInventory;

			inventory.Controller.AllEquipment.RemoveAll();
			foreach (var kpv in _itemStorage.EquipmentByType)
				foreach (var itemData in kpv.Value)
				{
					_equipmentInventoryService.AddEquipmentIfUnlocked(itemData.Args);
				}
		}
		
		private void RecreateFromCache()
		{
			var cache = _memoryPrefs.GetValue<RuntimeEquipmentArgs[]>(LIFE_SCOPE_PREFS_KEY);
			_memoryPrefs.DeleteKey(LIFE_SCOPE_PREFS_KEY);
			if (cache == null || cache.Length == 0)
				return;
			
			foreach (var cachedArgs in cache)
			{
				if (!cachedArgs.IsUnlocked)
					_equipmentInventoryService.AddNewEquipment(cachedArgs.EquipmentArgs);		
				_equipmentInventoryService.AddExistedEquipment(cachedArgs);
			}
			
			
		}
		
		private void RecreateActiveEquipment()
		{
			if (!_equipmentSaveAdapter.TryGetPresets(out var presets))
				return;
			
			foreach (var kpv in presets.Items)
				_currentContext.EquipmentInventory.Controller.ActiveEquipment.Select(kpv.Key, kpv.Value);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
			_destroySub?.Dispose();
		}
	}
}
