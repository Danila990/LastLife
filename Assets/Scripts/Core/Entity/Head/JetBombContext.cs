using System;
using AnnulusGames.LucidTools.Audio;
using Core.Effects;
using Core.Entity.Ai;
using Core.Entity.Ai.Sensor.ColliderSensor;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using NodeCanvas.Framework;
using SharedUtils;
using Sirenix.OdinInspector;
using Ui.Sandbox.Pointer;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Constants;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Entity.Head
{
	public class JetBombContext : LifeEntity, IInteractorVisiter, IColliderDetectionHandler, IExperienceEntity
	{
		public SimpleEffector Effector;
		[SerializeField] private NavMeshAgent _agent; 
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private Animator _animator;
		[SerializeField] private ColliderDetection _colliderDetection;
		
		[SerializeField] private Transform _lookAtTransform;
		[SerializeField] private Transform _model; 
		[SerializeField] private SerializedDamageArgs _explosionArgs;
		[SerializeField] private float _radius;
		[SerializeField] private float _explodeDistance;
		[SerializeField] private CharacterHealth _health;
		[SerializeField] private string _explosionFx;
		[SerializeField] private Blackboard _blackboard;
		
		[SerializeField, BoxGroup("Sfx")] private AudioClip _explosionSfx;
		[SerializeField, BoxGroup("Sfx")] private AudioClip _beepSound;
		[SerializeField, BoxGroup("Sfx")] private float _maxDistance = 500f;
		[SerializeField, BoxGroup("Sfx")] private float _volume = 1f;
		[SerializeField, BoxGroup("Sfx")] private float _spread = 1f;
		[SerializeField, BoxGroup("Sfx")] private AudioRolloffMode _mode = AudioRolloffMode.Logarithmic;
		
		[Inject] private readonly IOverlapInteractionService _overlapInteraction;
		[Inject] private readonly IEntityRepository _entityRepository;
		[Inject] private readonly PointerController _pointerController;
		
		private bool _isArmed;
		private bool _isActive;
		public float ExperienceCount => DIED_EXP_COUNT;
		public override Transform LookAtTransform => _lookAtTransform;
		public const float DIED_EXP_COUNT = 50f;

		private IAiTarget _aiTarget;
		private AudioPlayer _audioPlayer;
		private IDisposable _timer;
		private EntityContext _deathFrom;
		private SimpleEffectAnimator _effectAnimator;
		private EntityTarget _selfTarget;

		public Vector3 GetExpPosition() => MainTransform.position + new Vector3(0, MeshProvider.MainRenderer.localBounds.max.y);
		public override ILifeEntityHealth Health => _health;

		protected override void OnCreated(IObjectResolver resolver)
		{
			_health.Init();
			_health.SetContext(this);
			_health.OnDeath.Subscribe(OnDeath).AddTo(transform);
			_rigidbody.OnCollisionEnterAsObservable().Subscribe(OnCollision).AddTo(transform);
			SetState(false);
			_animator.SetTrigger(AHash.Hello);
			_animator.SetFloat("RandomCycleOffset", Random.value);
			_isActive = true;
			_colliderDetection.AddListener(this);
			Effector.SetContext(this);
			resolver.Inject(Effector);
			_pointerController.Add(this);

			Effector.Init();
			_effectAnimator = new SimpleEffectAnimator(_animator);
			_effectAnimator.Init(Effector);
			
			_timer = Observable
				.Timer(5f.ToSec())
				.TakeUntilDestroy(this)
				.TakeWhile(_ => !_agent.enabled)
				.Subscribe(_ =>
				{
					DeathTask().Forget();
				});

			_audioPlayer = AudioService.PlayNonQueue(_beepSound);
			_audioPlayer
					.SetPosition(transform.position)
					.SetVolume(_volume)
					.SetSpatialBlend(1f)
					.SetSpread(_spread)
					.SetMaxDistance(_maxDistance)
					.SetRolloffMode(_mode)
					.SetLoop();
		}

		private void OnCollision(Collision collision)
		{
			if (collision.gameObject.layer == Layers.EnvironmentLayer)
			{
				SetState(true);
				if (!_agent.isOnNavMesh)
					DeathTask().Forget();
			}
		}
		
		private void SetState(bool state)
		{
			_agent.enabled = state;
			_rigidbody.isKinematic = state;
		}

		private void OnDisable()
		{
			StopSound();
		}

		private void Update()
		{
			if(_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
			{
				_audioPlayer.SetPosition(transform.position);
			}
			
			if(!_isActive)
				return;

			_animator.SetBool(AHash.Walk, _aiTarget is { IsActive: true } && !Effector.IsEffected);
			
			if (_isArmed && _aiTarget is not { IsActive: true } && !_colliderDetection.IsScanning)
			{
				_colliderDetection.Scanning().Forget();
				return;
			}
			
			if(_aiTarget is not { IsActive: true } || _health.IsDeath || !_agent.enabled)
				return;


			RotateByNormal();
			TryExplode();
		}
		
		
		[Button]
		public void SetAiTarget(EntityContext context)
		{
			if (context.TryGetAiTarget(out var target))
				SetAiTarget(target);
		}
		
		public void SetAiTarget(IAiTarget target)
		{
			_isArmed = true;
			_blackboard.SetVariableValue("AiTarget", target);
			_aiTarget = target;
			_colliderDetection.StopScan();
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
			=> _health.DoDamage(ref args);

		private void RotateByNormal()
		{
			var hit = Physics.Raycast(_model.position + Vector3.up * 0.5f, Vector3.down, out var hitInfo, 5f, LayerMasks.Environment);
			if (hit)
			{
				var dot = Vector3.Dot(transform.forward, hitInfo.normal);
				var dir = transform.forward - hitInfo.normal * dot;
				
				_model.forward = dir.normalized;
			}
		}
		
		
		public override bool TryGetAiTarget(out IAiTarget result)
		{
			if (_health.IsDeath)
			{
				result = null;
				return false;
			}
			_selfTarget ??= new EntityTarget(this);
			result = _selfTarget;
			return true;
		}

		private void TryExplode()
		{
			var canExplode = (_aiTarget.MovePoint - transform.position).sqrMagnitude < _explodeDistance;
			if (canExplode)
				DoDeath();
		}

		private void DoDeath()
		{
			var args = new DamageArgs()
			{
				Damage = _health.CurrentHealth
			};
			_health.DoDamage(ref args);
		}

		public override void DoEffect(ref EffectArgs args)
		{
			Effector.DoEffect(args);
		}

		private void StopSound()
		{
			if(_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
			{
				_audioPlayer.Stop();
			}
		}
		
		private void OnDeath(DiedArgs obj)
		{
			StopSound();
			_deathFrom = obj.DiedFrom;
			Explode();
			DeathTask().Forget();
		}

		private void Explode()
		{
			_isArmed = false;

			if(!_isActive)
				return;
			
			VFXFactory.CreateAndForget(_explosionFx, transform.position, Vector3.up);
			
			if (AudioService.TryPlayQueueSound(_explosionSfx, Uid.ToString(), 0.1f, out var player))
			{
				player
					.SetPosition(transform.position)
					.SetSpatialBlend(1);
			}
			_overlapInteraction.OverlapSphere(this, transform.position, _radius);
		}

		private async UniTaskVoid DeathTask()
		{
			_isActive = false;
			_agent.enabled = false;
			_rigidbody.isKinematic = true;
			_model.gameObject.SetActive(false);
			DoDeath();
			
			await UniTask.Delay(5f.ToSec(), cancellationToken: destroyCancellationToken);
			OnDestroyed(_entityRepository);
			Destroy(gameObject);
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			_timer?.Dispose();
			base.OnDestroyed(entityRepository);
			entityRepository.RemoveGenericEntity((LifeEntity)this);
		}

		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta)
		{
			var args = _explosionArgs.GetArgs(this);
			var delta = (meta.Point - MainTransform.position).normalized;
			damage.HandleExplosion(ref args, meta.Point, delta, delta);
			return StaticInteractionResultMeta.InteractedBlocked;
		}
		
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
		{
			var args = _explosionArgs.GetArgs(this);
			var delta = (meta.Point - MainTransform.position).normalized;
			if (_deathFrom)
			{
				args.DamageSource = _deathFrom;
			}
			damagable.DoDamageExplosion(ref args,meta.Point,delta,delta);

			if (damagable is CharacterPartDamagable)
			{
				return new InteractionResultMeta { Interacted = true, HitBlock = true, DontCache = true };
			}
			
			return StaticInteractionResultMeta.InteractedBlocked;
		}
		public InteractionResultMeta Accept(EntityEffectable damagable, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
		
		#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, _radius);
		}
		#endif
		
		public void OnBufferUpdate(Collider[] colliders, in int size)
		{
			if (size <= 0)
				return;
			
			Util.ColliderByDistanceSort.SetPos(transform.position);
			Array.Sort(colliders, 0, size, Util.ColliderByDistanceSort);
			
			for (var i = 0; i < size; i++)
			{
				var col = colliders[i];
				col.transform.TryGetComponent(out IEntityAdapter entityAdapter);
				if (entityAdapter != null && entityAdapter.Entity)
				{
					SetAiTarget(entityAdapter.Entity);
					return;
				}
			}
		}
	}
}
