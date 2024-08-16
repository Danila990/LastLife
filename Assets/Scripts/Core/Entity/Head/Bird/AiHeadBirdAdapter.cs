using AnnulusGames.LucidTools.Audio;
using Core.Entity.Ai;
using Core.Entity.Ai.AiItem;
using Core.Entity.Ai.Movement;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using Core.InputSystem;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Head.Bird
{
	public class AiHeadBirdAdapter : AiHeadAdapter
	{
		[SerializeField] private MonoItemProvider _monoItemProvider;
		[SerializeField] private AudioClip _bigExplDamage;
		[SerializeField] private AudioClip _smallExplDamage;
		private bool _canPlayDmgSound;
		private AudioPlayer _smallExpSoundPlayer;

		protected override void OnConstruct()
		{
			_monoItemProvider.Created(Resolver);
			Resolver.Resolve<IPlayerSpawnService>().PlayerCharacterAdapter.ContextChanged.Subscribe(PlayerContextChanged);
		}
		
		private void PlayerContextChanged(CharacterContext entity)
		{
			if (!entity)
				return;				
			var target = _aiBlackboard.GetVariableValue<IAiTarget>("AiTarget");
			
			if (target is { IsActive: true })
				return;
			
			if (!entity.TryGetAiTarget(out var aiTarget))
				return;
			
			_aiBlackboard.SetVariableValue("AiTarget", aiTarget);
		}

		public override void OnBossRoomOpened()
		{
			CurrentContext.OnStartFight();
			_behaviourTreeOwner.StopBehaviour();
			_aiMovementController.SetDestination(new Vector3(-1.49f,5f,45.87f));

			BossInitializedAsync().Forget();
		}
		
		private async UniTaskVoid BossInitializedAsync()
		{
			var waiting = UniTask.WaitUntil(() => _aiMovementController.AgentStatusType == AgentStatusType.Waiting);
			var delay = UniTask.Delay(10f.ToSec());
			await UniTask.WhenAny(waiting, delay);
			_aiBlackboard.SetVariableValue("InBossRoom", false);
			_behaviourTreeOwner.StartBehaviour();
		}

		public override void SetEntityContext(HeadContext context)
		{
			foreach (var monoAiItem in _monoItemProvider.MonoItems)
			{
				monoAiItem.Created(context);
			}
			var headBird = (HeadBird)context;
			base.SetEntityContext(context);
			
			_aiBlackboard.SetVariableValue("Animator", context.Animator);
			_aiBlackboard.SetVariableValue("HeartDamagable", headBird.HeartDamagable.gameObject);
		}

		protected override void OnGetDamage(DamageArgs args)
		{
			if (args.DamageType is DamageType.Explosion)
			{
				if (args.Damage > 0)
				{
					GetBigDamage();
					_smallExpSoundPlayer?.Stop();
					_smallExpSoundPlayer = null;
					_canPlayDmgSound = false;
				
					CurrentContext
						.AudioService
						.PlayNonQueue(_bigExplDamage)
						.SetPosition(CurrentContext.LookAtTransform.position)
						.OnComplete(ReleaseSound)
						.SetSpatialBlend(1);
				}
				else
				{
					if (!_canPlayDmgSound)
						return;
				
					_canPlayDmgSound = false;
					_smallExpSoundPlayer = CurrentContext
						.AudioService
						.PlayNonQueue(_smallExplDamage)
						.SetPosition(CurrentContext.LookAtTransform.position)
						.SetSpatialBlend(1)
						.OnComplete(ReleaseSound);
				}
			}
		}
		
		private void ReleaseSound()
		{
			_canPlayDmgSound = true;
		}

		[Button]
		private void GetBigDamage()
		{
			_aiBlackboard.SetVariableValue("Interrupted", true);
			CurrentContext.Animator.SetTrigger(AHash.HeavyImpact);
			//CurrentContext.Health.DoDamage(ref args);
		}
	}
}