using System.Linq;
using Core.Entity.Characters;
using Core.Equipment.Data;
using Core.Equipment.Inventory;
using Core.InputSystem;
using Core.Services;
using Db.ObjectData;
using UnityEngine;

namespace Core.Equipment
{
	public interface IEquipmentInventoryService
	{
		public void AddNewEquipment(IEquipmentArgs equipmentArgs);
		public void AddEquipmentIfUnlocked(IEquipmentArgs equipmentArgs);
		public void AddExistedEquipment(RuntimeEquipmentArgs args);
	}

	public class EquipmentInventoryService : IEquipmentInventoryService
	{
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _unlockService;

		
		private CharacterContext CurrentContext => _playerSpawnService.PlayerCharacterAdapter.CurrentContext;

		public EquipmentInventoryService(
			IPlayerSpawnService playerSpawnService,
			IItemStorage itemStorage,
			IItemUnlockService unlockService)
		{
			_playerSpawnService = playerSpawnService;
			_itemStorage = itemStorage;
			_unlockService = unlockService;
		}

		public void AddNewEquipment(IEquipmentArgs equipmentArgs)
		{
			if(!CurrentContext)
				return;

			var isUnlocked = _unlockService.IsUnlocked(GetObjectDataByEquipment(equipmentArgs));
			var isLifeTimeScope  = !isUnlocked;
			var runtimeArgs = new RuntimeEquipmentArgs(isUnlocked, isLifeTimeScope, equipmentArgs);
			CurrentContext.EquipmentInventory.Controller.AllEquipment.AddPart(runtimeArgs);
		}
		
		public void AddEquipmentIfUnlocked(IEquipmentArgs equipmentArgs)
		{
			if(!CurrentContext)
				return;
			
			if (!_unlockService.IsUnlocked(GetObjectDataByEquipment(equipmentArgs)))
				return;
				
			AddNewEquipment(equipmentArgs);
		}
		
		public void AddExistedEquipment(RuntimeEquipmentArgs args)
		{
			if(!CurrentContext)
				return;
			
			CurrentContext.EquipmentInventory.Controller.AllEquipment.ChangePart(args);
		}

		private ObjectData GetObjectDataByEquipment(IEquipmentArgs equipmentArgs)
		{
#if UNITY_EDITOR
			if (_itemStorage.EquipmentByType[equipmentArgs.PartType].All(x => x.Id != equipmentArgs.Id))
			{
				Debug.LogError(equipmentArgs.Id + " not found in " + equipmentArgs.PartType);
				return null;
			}
#endif
			return _itemStorage.EquipmentByType[equipmentArgs.PartType].First(x => x.Id == equipmentArgs.Id);
		}

	}
}
