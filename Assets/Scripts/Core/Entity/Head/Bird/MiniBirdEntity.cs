using System;
using Core.Actions;
using Core.Effects;
using Core.Entity.Ai;
using Core.Entity.Ai.Sensor.ColliderSensor;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic;
using Core.Entity.Repository;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using LitMotion;
using NodeCanvas.Framework;
using SharedUtils;
using Sirenix.OdinInspector;
using Ui.Sandbox.Pointer;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Head.Bird
{
	public class MiniBirdEntity : LifeEntity, IColliderDetectionHandler, IExperienceEntity
	{
		[SerializeField] private CharacterHealth _health;
		
		[SerializeField] private SimpleEffector _effector;
		[SerializeField] private NavMeshAgent _agent; 
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private Animator _animator;
		[SerializeField] private Blackboard _blackboard;
		[SerializeField] private MeshProvider _meshProvider;
		[SerializeField] private AudioClip _deathClip;
		
		[SerializeField] private ColliderDetection _colliderDetection;
		[SerializeField] private MeleePunchMonoAction _meleePunchMonoAction;
		
		[Inject] private readonly IOverlapInteractionService _overlapInteraction;
		[Inject] private readonly IEntityRepository _entityRepository;
		[Inject] private readonly PointerController _pointerController;
		
		private IAiTarget _aiTarget;
		private EntityTarget _selfTarget;
		private SimpleEffectAnimator _effectAnimator;
		
		private bool _isDestroyed;
		
		public float ExperienceCount => 50f;
		public Vector3 GetExpPosition() => MainTransform.position + new Vector3(0, MeshProvider.MainRenderer.localBounds.max.y);

		public override ILifeEntityHealth Health => _health;

		protected override void OnCreated(IObjectResolver resolver)
		{
			_health.Init();
			_health.SetContext(this);
			_health.OnDeath.Subscribe(OnDeath).AddTo(this);
			_meleePunchMonoAction.Init(_overlapInteraction);
 			_colliderDetection.AddListener(this);
			_effector.SetContext(this);
			_effector.Init();
			_effectAnimator = new(_animator);
			_effectAnimator.Init(_effector);
			resolver.Inject(_effector);
			_pointerController.Add(this);
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			entityRepository.RemoveGenericEntity((LifeEntity)this);
		}

		private void OnDeath(DiedArgs obj)
		{
			_animator.SetBool(AHash.Death, true);
			
			LMotion
				.Create(_agent.baseOffset, 0, 0.5f)
				.Bind(f => _agent.baseOffset = f)
				.AddTo(this);

			AudioService
				.PlayNonQueue(_deathClip)
				.SetPosition(transform.position)
				.SetSpatialBlend(1);
			
			DeathTask().Forget();
			OnDestroyed(_entityRepository);
		}
		
		public async UniTaskVoid DeathTask()
		{
			if (_isDestroyed)
				return;
			
			_isDestroyed = true;
			_rigidbody.isKinematic = false;
			_agent.enabled = false;
			
			await UniTask
				.Delay(4f.ToSec(), cancellationToken: destroyCancellationToken);
			
			await LMotion.Create(0f, 1f, 3f)
				.Bind(SetDissolve).ToUniTask(destroyCancellationToken);
			
			await UniTask
				.Delay(0.1f.ToSec(), cancellationToken: destroyCancellationToken);
			
			Destroy(gameObject, 5f);
		}
		
		private void SetDissolve(float value)
		{
			foreach (var render in _meshProvider.AllRenderers)
			{
				foreach (var material in render.materials)
				{
					material.SetFloat(ShHash.DissolveSlider, value);
				}
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
		
		private void Update()
		{
			if (Health.IsDeath)
				return;
			
			if (_aiTarget is not { IsActive: true } && !_colliderDetection.IsScanning)
			{
				_colliderDetection.Scanning().Forget();
				return;
			}
			_animator.SetBool(AHash.Walk, _aiTarget is { IsActive: true } && !_effector.IsEffected);
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			_health.DoDamage(ref args);
			
			if(args.DamageSource is null || args.DamageSource.Uid == Uid) 
				return;

			if (!args.DamageSource.TryGetAiTarget(out var newAiTarget))
				return;
			
			var target = _blackboard.GetVariableValue<IAiTarget>("AiTarget");
			
			if (target is { IsActive: true })
				return;

			SetAiTarget(newAiTarget);
		}

		[Button]
		public void SetAiTarget(EntityContext context)
		{
			if (context.TryGetAiTarget(out var target))
				SetAiTarget(target);
		}
		
		public void SetAiTarget(IAiTarget target)
		{
			_blackboard.SetVariableValue("AiTarget", target);
			_aiTarget = target;
			_colliderDetection.StopScan();
		}
		
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