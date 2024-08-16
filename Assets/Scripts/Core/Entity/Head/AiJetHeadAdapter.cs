using AnnulusGames.LucidTools.Audio;
using Core.Entity.Ai;
using Core.Entity.Ai.AiItem;
using Core.Entity.Ai.Movement;
using Core.Entity.Characters.Adapters;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Head
{
	public class AiJetHeadAdapter : AiHeadAdapter, IAiItemUseListener
	{
		[BoxGroup("Sfx"), SerializeField] private AudioClip _engineSound;
		[BoxGroup("Sfx"), SerializeField] private float _maxDistance = 500f;
		[BoxGroup("Sfx"), SerializeField] private float _volume = 1f;
		[BoxGroup("Sfx"), SerializeField] private float _spread = 1f;
		[BoxGroup("Sfx"), SerializeField] private AudioRolloffMode _mode = AudioRolloffMode.Logarithmic;
		
		private FlyingAiMovementController _flyingMovementController;
		private AudioPlayer _audioPlayer;
		
		
		public FlyingAiMovementController MovementController => _flyingMovementController;

		public override void SetEntityContext(HeadContext context)
		{
			base.SetEntityContext(context);
			
			if (context.AudioService.TryPlayQueueSound(_engineSound, context.Uid.ToString(), 0.1f,
				    out _audioPlayer))
			{
				_audioPlayer
					.SetPosition(transform.position)
					.SetVolume(_volume)
					.SetSpatialBlend(1f)
					.SetSpread(_spread)
					.SetMaxDistance(_maxDistance)
					.SetRolloffMode(_mode)
					.SetLoop();
			}
			
			_flyingMovementController = (FlyingAiMovementController) _aiMovementController;
		}

		private void Update()
		{
			if(_audioPlayer.state == AudioPlayer.State.Stop)
				return;

			_audioPlayer.SetPosition(transform.position);
		}

		public override void OnDied()
		{
			base.OnDied();
			StopEngine();
		}

		private void OnDisable()
		{
			StopEngine();
		}

		private void StopEngine()
		{
			if(_audioPlayer?.state == AudioPlayer.State.Stop)
				return;
			
			_audioPlayer?.Stop();
		}

		public override void OnBossRoomOpened()
		{
			CurrentContext.OnStartFight();
			_behaviourTreeOwner.PauseBehaviour();
			_flyingMovementController.SetPathManually(new Vector3[]
			{
				new Vector3(0.36f,2f,92.07f),
				new Vector3(-1.49f,2f,81.87f),
				new Vector3(-1.49f,4f,65.87f),
				new Vector3(-1.49f,5f,60.87f),
			});
			
			BossInitializedAsync().Forget();
		}

		private async UniTaskVoid BossInitializedAsync()
		{
			var waiting = UniTask.WaitUntil(() => _flyingMovementController.AgentStatusType == AgentStatusType.Waiting);
			var delay = UniTask.Delay(10f.ToSec());
			await UniTask.WhenAny(waiting, delay);
			_aiBlackboard.SetVariableValue("InBossRoom", false);
			_behaviourTreeOwner.StartBehaviour();
		}
		
		public void OnUse(IAiItem aiItem, IAiTarget aiTarget)
		{
			_flyingMovementController.IsRotateByPath = false;
			_flyingMovementController.SetRotationTarget(aiTarget);
		}
		
		public void OnEndUse(IAiItem aiItem)
		{
			_flyingMovementController.IsRotateByPath = true;
		}
	}
}