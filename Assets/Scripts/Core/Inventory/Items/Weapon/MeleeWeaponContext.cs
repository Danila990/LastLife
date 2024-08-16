using System;
using AnnulusGames.LucidTools.Audio;
using Core.Actions;
using Core.Boosts;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.EntityAnimation;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Core.Inventory.Origins;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Inventory.Items.Weapon
{
	public class MeleeWeaponContext : WeaponContext, IInteractorVisiter
	{
		[TitleGroup("MainSettings/Melee")]
		[SerializeField] private float _attackDelay;
		[SerializeField] private float _attackRadius;
		[SerializeField] private float _hardMeleeThreshold; 
		[SerializeField] private SerializedDamageArgs _args; 
		[SerializeField] private float _animTriggerTime;
		[SerializeField] private AnimationClip _attackClip; 
		[SerializeField] private AnimationClip _kickClip; 
		[SerializeField] private AudioClip[] _attackSound;
		
		private SimpleTimerAction _timerAction;
		private IMeleeCharacter _characterContext;
		private bool _inAttack;

		[Inject] private readonly IOverlapInteractionService _overlapInteractionService;
		private CharacterAnimatorAdapter _characterAnimatorAdapter;
		private BaseOriginProvider _baseOriginProvider;

		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			_characterContext = (IMeleeCharacter)Owner;
			_timerAction = new SimpleTimerAction(_attackDelay);
			_timerAction.SetAction(SimpleAttack);
		}

		public override void ItemInit(IOriginProxy inventory)
		{
			base.ItemInit(inventory);
			_baseOriginProvider = inventory.Origin;
		}

		public void Attack()
		{
			AttackTask().Forget();
		}
		
		private void Update()
		{
			var deltaTime = Time.deltaTime;
			_timerAction.Tick(ref deltaTime);
		}

		public void UseSimpleAttack(bool pressed)
		{
			_timerAction.CanUse(pressed);
		}
		
		private void SimpleAttack()
		{
			_characterAnimatorAdapter ??= _characterContext.AnimatorAdapter;
			var rnd = Random.value < 0.5f;
			_timerAction.SetTime(rnd ? 1.4f : 1.2f);
			_timerAction.Reset();
			AttackAsync(_characterAnimatorAdapter.PlayAction(_attackClip, rnd ? "Hit1" : "Hit2", rnd ? "Punch1" : "Punch2", _animTriggerTime)).Forget();
		}

		public void UseLegAttack(IAnimationEntityAction entityAction)
		{
			_characterAnimatorAdapter ??= _characterContext.AnimatorAdapter;
			_timerAction.Reset();
			AttackAsyncLeg(entityAction.UseFromEvent).Forget();
		}

		private async UniTaskVoid AttackAsync(UniTask<bool> awaitTask)
		{
			if(_inAttack) 
				return;
			
			_inAttack = true;
			var res = await awaitTask;
			_inAttack = false;

			if(!res) 
				return;
			
			AttackInteraction(_baseOriginProvider.GetMeleeOrigin());
		}
		
		private async UniTaskVoid AttackAsyncLeg(Action action)
		{
			if(_inAttack) 
				return;
			
			_inAttack = true;
			var res = await _characterAnimatorAdapter.LegAttack(_kickClip, "LegAttack","FPVKick", _animTriggerTime, action);
			if (res)
			{
				AttackInteraction(_baseOriginProvider.GetMeleeOrigin());
			}
			await UniTask.Delay(_kickClip.length.ToSec() * .4f, cancellationToken: destroyCancellationToken);
			_inAttack = false;
		}
		
		public async UniTaskVoid KickTask()
		{
			_characterContext.CharacterAnimatorRef.Animator.SetBool(AHash.IsMeleeAttack, true);
			await _characterContext.CharacterAnimatorRef.PlayAction(_kickClip, _animTriggerTime);
			_characterContext.CharacterAnimatorRef.Animator.SetBool(AHash.IsMeleeAttack, false);
			AttackInteraction(_baseOriginProvider.GetMeleeOrigin());
		}
		
		private async UniTaskVoid AttackTask()
		{
			await _characterContext.CharacterAnimatorRef.PlayAction(_attackClip, _animTriggerTime);
			AttackInteraction(_baseOriginProvider.GetMeleeOrigin());
		}

		private void AttackInteraction(Vector3 point, IInteractorVisiter visiter = null)
		{
			_overlapInteractionService.OverlapSphere(visiter ?? this,
				point, 
				_attackRadius,
				_characterContext.Uid,LayerMasks.HitObjectMask);
			
			if (_attackSound.Length > 0)
			{
				LucidAudio.PlaySE(_attackSound.GetRandom()).SetPosition(_characterContext.MainTransform.position).SetSpatialBlend(1);
			}
		}
		
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
		{
			var damageArgs = _args.GetArgs(_characterContext as EntityContext);

			var additionalDamage = 0f; 
			var additionalMeleeDamage = 0f; 
			var additionalHitForce = 0f;
			_characterContext.StatsProvider?.Stats.GetValue(StatType.AllDamage, out additionalDamage);
			_characterContext.StatsProvider?.Stats.GetValue(StatType.MeleeDamage, out additionalMeleeDamage);
			_characterContext.StatsProvider?.Stats.GetValue(StatType.MeleeHitForce, out additionalHitForce);
			
			damageArgs.Damage += additionalDamage + additionalMeleeDamage;
			damageArgs.DamageType = damageArgs.Damage >= _hardMeleeThreshold ? DamageType.HardMelee : DamageType.Melee;
			damageArgs.HitForce += additionalHitForce;
			Vector3 delta = Vector3.zero;
			if (Owner)
			{
				delta = Owner.MainTransform.position - meta.Point;
				delta.y = -.35f;
			}

			damagable.DoDamageMelee(ref damageArgs, meta.Point, delta.normalized);			
			return new InteractionResultMeta(true, true);
		}
		
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
		{
			if (!environment.AcceptMelee) return StaticInteractionResultMeta.Default;
			var damageArgs = _args.GetArgs(_characterContext as EntityContext);
			environment.OnHit(ref damageArgs,ref meta);
			var behaviour = environment.BehaviourData.GetBehaviour(DamageType.Range);
			if (behaviour.TryGetSound(out var clip))
			{
				if (AudioService.TryPlayQueueSound(clip, "ground" + Owner.Uid, 0.1f, out var player))
				{
					player
						.SetPosition(meta.Point)
						.SetVolume(0.2f)
						.SetSpatialBlend(1);
				}
			}
			return StaticInteractionResultMeta.InteractedBlocked;
		}
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
	}
}