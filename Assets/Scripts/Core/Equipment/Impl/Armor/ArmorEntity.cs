using Core.Entity.Characters;
using Core.Entity.InteractionLogic.Interactions;
using Core.Equipment.Data;
using Core.Equipment.Inventory;
using Core.HealthSystem;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Equipment.Impl.Armor
{
	public class ArmorEntity : EquipmentEntityContext, IArmorPart
	{
		[field:SerializeField] public EntityDamagable Damagable { get; private set; }

		[SerializeField] private Health _health;
		public Health Health => _health;
		
		private ArmorItemArgs _currentItemArgs;

		protected override void OnCreated(IObjectResolver resolver)
		{
			_health = new Health();
			_health.Init();
			_health.OnDeath.Subscribe(OnDeath).AddTo(this);
		}
		
		public override void ChangeCurrentArgs(in IEquipmentArgs args)
		{
			if (args is ArmorItemArgs bulletproofArgs)
			{
				_currentItemArgs = bulletproofArgs;
				_health.SetCurrentHealth(_currentItemArgs.Health);
				_health.SetMaxHealth(_currentItemArgs.Health);
			}
		}
		public override IEquipmentArgs GetItemArgs()
		{
			return _currentItemArgs;
		}
		
		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			if(_health.IsDeath)
				return;
			
			_health.DoDamage(ref args);
			_currentItemArgs.Health = _health.CurrentHealth;
		}

		private void OnDeath(DiedArgs _)
		{
			if (Inventory.TryGetController(out var controller))
			{
				var unequipArgs = new UnequipArgs(UnequipReason.ByDestroy, _currentItemArgs.PartType);
				controller.ActiveEquipment.Deselect(ref unequipArgs);
			}
		}

		protected override void OnPutOnInternal()
		{
			if (Inventory.TryGetParts(PartType, out var parts))
			{
				foreach (var part in parts)
					part.SetArmorRef(this);
			}
		}

		protected override void OnTakeOffInternal()
		{
			_health?.Dispose();
		}
		

		protected override void PlaceCosmetic()
		{
			if (Inventory.TryGetOrigin(PartType, out var data))
			{
				var placementData = data.GetOffset(_currentItemArgs.FactoryId);

				MainTransform.SetParent(placementData.Origin);
				MainTransform.localPosition = placementData.Origin.localPosition + placementData.Offset;
				MainTransform.localEulerAngles = placementData.Rotation;
			}
			
			Equipmentizer.AttachTo(Inventory.Renderer);
		}
	}
}
