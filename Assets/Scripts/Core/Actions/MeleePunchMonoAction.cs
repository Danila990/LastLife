using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Actions
{
	public class MeleePunchMonoAction : MonoBehaviour, IInteractorVisiter
	{
		[SerializeField] private float _castRadius = 0.5f;
		[SerializeField] private SerializedDamageArgs _args;
		[field: SerializeField] public EntityContext Sender { get; set; }
		
		[ValueDropdown("@LayerMasks.GetLayersMasks()")]
		[SerializeField] 
		private int _layer = 8;
		
		private IOverlapInteractionService _overlapInteractionService;

		public void Init(IOverlapInteractionService overlapInteractionService)
		{
			_overlapInteractionService = overlapInteractionService;
		}
		
		public void CastAttack(Vector3 castPoint)
		{
			AttackInteraction(castPoint);
		}
		
		private void AttackInteraction(Vector3 castPoint, IInteractorVisiter visiter = null)
		{
			_overlapInteractionService.OverlapSphere(visiter ?? this,
				castPoint, 
				_castRadius,
				Sender.Uid,
				_layer);
		}
		
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
		{
			var damageArgs = _args.GetArgs(Sender);
			damageArgs.DamageType = _args.DamageType;
			damagable.DoDamageMelee(ref damageArgs, meta.Point, meta.Normal);			
			return new InteractionResultMeta(true, true);
		}
		
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
	}
}