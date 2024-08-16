using System;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Impl.Hat
{
	public class VikingHatEquipmentEntity : HatEquipmentEntity
	{
		[SerializeField, BoxGroup("Params")] private float _meleeDamage;
		[SerializeField, BoxGroup("Params")] private float _hitForce;
		
		private PlayerCharacterAdapter _adapter;
		private IDisposable _stat;
		private IDisposable _meleeForce;

		protected override void OnPutOnInternal()
		{
			base.OnPutOnInternal();
			if(Owner == null)
				return;
			
			if(Owner.Adapter is not PlayerCharacterAdapter adapter)
				return;
			
			_adapter = adapter;
			_stat = _adapter.StatsProvider.Stats.IncreaseStats(StatType.MeleeDamage, _meleeDamage);
			_meleeForce = _adapter.StatsProvider.Stats.IncreaseStats(StatType.MeleeHitForce, _hitForce);
		}

		protected override void OnTakeOffInternal()
		{
			base.OnTakeOffInternal();
			_stat?.Dispose();
			_meleeForce?.Dispose();
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_stat?.Dispose();
			_meleeForce?.Dispose();
		}
	}
}
