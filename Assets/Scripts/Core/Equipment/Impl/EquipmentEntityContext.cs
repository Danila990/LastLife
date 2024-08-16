using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Repository;
using Core.Equipment.Data;
using Core.Equipment.Inventory;
using UnityEngine;
using CameraType = Core.CameraSystem.CameraType;

namespace Core.Equipment.Impl
{
	public abstract class EquipmentEntityContext : EntityContext
	{
		[SerializeField] private EquipmentPartType _partType;
		[SerializeField] private Equipmentizer _equipmentizer;

		public CharacterContext Owner { get; private set; }
		protected IEquipmentInventory Inventory;
		
		public EquipmentPartType PartType => _partType;
		public Equipmentizer Equipmentizer => _equipmentizer;
		public bool IsEquipped { get; private set; }
		
		public abstract IEquipmentArgs GetItemArgs();
		public virtual void ChangeCurrentArgs(in IEquipmentArgs args) { }

		
#region Callbacks

		public void OnEquip(CharacterContext owner, IEquipmentInventory inventory)
		{
			if (IsEquipped)
				return;
			
			Owner = owner;
			Inventory = inventory;
			IsEquipped = true;
			OnPutOnInternal();
			PlaceCosmetic();
		}

		public void OnUnequip()
		{
			if (!IsEquipped)
				return;

			IsEquipped = false;
			OnTakeOffInternal();
			Owner = null;
			Inventory = null;
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			IsEquipped = false;
		}

		public void Tick()
		{
			if (Owner)
				OnTick();
		}

#endregion

#region VirtualCallbacks
		protected virtual void OnPutOnInternal() { }
		
		protected virtual void OnTakeOffInternal() { }

		protected virtual void PlaceCosmetic() { }
		
		protected virtual void OnTick() { }
#endregion

		public virtual void OnCameraChanged(CameraType cameraType)
		{
			transform.localEulerAngles = Vector3.zero;
			gameObject.SetActive(cameraType != CameraType.Fpv);
		}
	}

}
