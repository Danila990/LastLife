using System;
using Core.Entity;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Projectile
{
	[Serializable]
	public class InternalExplosionVisitor : IInteractorVisiter, IExplosionVisiter
	{
		public SerializedDamageArgs Args;
		public bool NoDamage;
		private EntityContext _owner;

		public void SetOwner(EntityContext owner) => _owner = owner;

		public float ExtraDmg;

		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta)
		{
			if (NoDamage) return StaticInteractionResultMeta.Default;
			var args = Args.GetArgs(_owner);
			var delta = (meta.Point - _owner.MainTransform.position).normalized;
			damage.HandleExplosion(ref args, meta.Point, delta, delta);
			return StaticInteractionResultMeta.InteractedBlocked;
		}
		protected virtual void OnAccept(EntityDamagable damage, Vector3 metaPoint) { }
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
		{
			if (NoDamage) return StaticInteractionResultMeta.Default;
			var args = Args.GetArgs(_owner);
			args.Damage += ExtraDmg;
			var delta = (meta.Point - _owner.MainTransform.position).normalized;
			damagable.DoDamageExplosion(ref args,meta.Point,delta,delta);
			OnAccept(damagable, meta.Point);

			if (damagable.DontCashDamagedUIDs)
			{
				return new InteractionResultMeta { Interacted = true, HitBlock = true, DontCache = true };
			}
			return StaticInteractionResultMeta.InteractedBlocked;
		}
	}
}
