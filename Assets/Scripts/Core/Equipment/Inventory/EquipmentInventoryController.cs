using System;
using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Player.MovementFSM;
using UniRx;
using VContainer;
using CameraType = Core.CameraSystem.CameraType;

namespace Core.Equipment.Inventory
{
	public class EquipmentInventoryController : IDisposable
	{
		#region Fields
		[Inject] private readonly ICameraService _cameraService;

		public readonly ActiveEquipment ActiveEquipment;
		public readonly AllEquipment AllEquipment;
		
		private readonly CharacterContext _owner;
		private readonly EquipmentInventory _inventory;
		private readonly IEntityRepository _entityRepository;
		
		#endregion

		public EquipmentInventoryController(
			Func<IEquipmentArgs, EquipmentEntityContext> onSelectCallback,
			Action<EquipmentEntityContext> onRemoveCallback,
			CharacterContext owner,
			EquipmentInventory inventory, 
			IEntityRepository entityRepository,
			AllEquipment allEquipment)
		{
			_owner = owner;
			_inventory = inventory;
			_entityRepository = entityRepository;

			AllEquipment = allEquipment;
			ActiveEquipment = new ActiveEquipment(allEquipment, owner, inventory, onSelectCallback, onRemoveCallback);
			ActiveEquipment.OnEquip.Subscribe(OnPutOn).AddTo(_owner);
		}

		private void SetRefToFsm(PlayerMovementStateMachine fsm)
		{
			fsm.ReusableData.EquipmentInventory = _inventory;
		}
		
		public void Dispose()
		{
			ActiveEquipment.Dispose();
			ActiveEquipment?.Dispose();
		}
		
		#region Handlers
		public void SetAdapter(PlayerCharacterAdapter adapter)
		{
			SetRefToFsm(adapter.MovementStateMachine);
			_cameraService.OnCameraTypeChanged.Subscribe(OnCameraModeChanged).AddTo(_owner);
		}
		
		private void OnCameraModeChanged(CameraType cameraType)
		{
			foreach (var part in ActiveEquipment.Items)
			{
				part.OnCameraChanged(cameraType);
			}
		}

		private void OnPutOn(EquipmentEntityContext context)
		{
			context.gameObject.SetActive(_cameraService.IsThirdPerson);
		}
		
		public void OnDeath()
		{
			Dispose();
		}
		
		public void OnDestroyed()
		{
			foreach (var item in ActiveEquipment.Items)
			{
				if(!item)
					continue;
				item.OnDestroyed(_entityRepository);
				UnityEngine.Object.Destroy(item.gameObject);
			}
			Dispose();
		}
		#endregion
	}
	public struct RuntimeEquipmentArgs
	{
		public readonly bool IsUnlocked;
		public readonly bool IsLifeTimeScope;
		public bool IsBlocked;
		public readonly IEquipmentArgs EquipmentArgs;

		public RuntimeEquipmentArgs(bool isUnlocked, bool isLifeTimeScope,  IEquipmentArgs equipmentArgs)
		{
			IsUnlocked = isUnlocked;
			IsLifeTimeScope = isLifeTimeScope;
			EquipmentArgs = equipmentArgs;
			IsBlocked = false;
		}
	}

	public enum UnequipReason
	{
		ByPlayer,
		BySelf,
		ByDestroy,
	}
	
	public struct UnequipArgs
	{
		public readonly UnequipReason Reason;
		public readonly EquipmentPartType PartType;
		public EquipmentEntityContext Context;
		
		public UnequipArgs(UnequipReason reason, EquipmentPartType partType)
		{
			Reason = reason;
			PartType = partType;
			Context = null;
		}
	}
}
