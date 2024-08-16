using Core.Entity.Characters;
using Core.HealthSystem;
using UnityEngine;
using Utils;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class PhysicDamageDealer : MonoBehaviour, IInteractorVisiter
	{
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private float _threshold;
		[SerializeField] private SerializedDamageArgs _damage;
		[SerializeField] private EntityContext _context;
        

		protected void OnCollisionEnter(Collision collision)
		{
			if (_rigidbody.velocity.magnitude * _rigidbody.mass < _threshold)
				return;
			
			VisiterUtils.CollisionVisit(collision, this);
		}
        
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
		{
			var args = _damage.GetArgs(_context);
			damagable.DoDamageWithEffects(ref args,meta.Point, meta.Normal, DamageType.Impact);
			return StaticInteractionResultMeta.InteractedBlocked;
		}
		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta)
			=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta)
			=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
			=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityEffectable damagable, ref InteractionCallMeta meta)
			=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta)
			=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)
			=> StaticInteractionResultMeta.Default;
	}
}
