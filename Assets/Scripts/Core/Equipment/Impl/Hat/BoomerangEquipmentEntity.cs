using System;
using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Core.Actions;
using Core.Entity.Ai;
using Core.Entity.Ai.Sensor.ColliderSensor;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using CameraType = Core.CameraSystem.CameraType;

namespace Core.Equipment.Impl.Hat
{
	public class BoomerangEquipmentEntity : HatEquipmentEntity, IColliderDetectionHandler
	{
		[SerializeField, BoxGroup("SceneRefs")] private OverlapDetection _colliderDetection;
		[SerializeField, BoxGroup("SceneRefs")] private Transform _model;
		[SerializeField, BoxGroup("SceneRefs")] private MeleePunchMonoAction _attackAction;
		
		[SerializeField, BoxGroup("Params")] private float _attackInterval;
		[SerializeField, BoxGroup("Params")] private float _attackmMoveSpeed;
		[SerializeField, BoxGroup("Params")] private float _followSpeed = .2f;
		[SerializeField, BoxGroup("Params")] private float _followMlp = .2f;
		
		[SerializeField, BoxGroup("Params")] private float _height;
		[SerializeField, BoxGroup("Params")] private float _amplitude;
		[SerializeField, BoxGroup("Params")] private Vector3 _offset;
		[SerializeField, BoxGroup("Params")] private float _inactiveRotationSpeed;
		[SerializeField, BoxGroup("Params")] private float  _rotationAroundPlayerSpeed;
		[SerializeField, BoxGroup("Params")] private float  _radius = 1.5f;
		
		[SerializeField, BoxGroup("Visual")] private MeshRenderer _meshRenderer;
		[SerializeField, BoxGroup("Visual"), ColorUsage(true,true)] private Color _newColor;
		
		[SerializeField, BoxGroup("Sound")] private AudioClip _clip;
		[SerializeField, BoxGroup("Sound")] private float _volume;
		
		private IAiTarget _currentAiTarget;
		private Transform _originOrigin;
		private Vector3 _velocity = Vector3.one;
		private Transform _originData;
		public bool InAttack { get; set; }
		public bool HasTarget  { get; private set; }
		
		private Vector3 _lastTargetPosition;
		private ISubscriber<MessageDamageEvent> _subscriber;
		private IDisposable _disposable;

		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			_colliderDetection.AddListener(this);
			_attackAction.Init(resolver.Resolve<IOverlapInteractionService>());
			_subscriber = resolver.Resolve<ISubscriber<MessageDamageEvent>>();
		}

		protected override void OnPutOnInternal()
		{
			_colliderDetection.Scanning().Forget();
			var contains = Inventory.TryGetOrigin(PartType, out var data);
			_originData = data.GetOffset(CurrentItemArgs.FactoryId).Origin;
			MainTransform.position = GetPosition();
			MainTransform.SetParent(null);
			Debug.Assert(contains);
			_disposable?.Dispose();
			_disposable = _subscriber.Subscribe(OnDamage);
			_attackAction.Sender = Owner;
		}
		
		private void OnDamage(MessageDamageEvent obj)
		{
			if (obj.DamageArgs.DamageSource is null || obj.AffectedEntity is null)
				return;
			
			var delta = obj.DamageArgs.DamageSource.MainTransform.position - obj.AffectedEntity.MainTransform.position;
			if (_currentAiTarget is null &&
			    obj.DamageArgs.DamageSource.Uid == Owner?.Uid &&
			    obj.AffectedEntity.TryGetAiTarget(out var target) &&
			    delta.magnitude < _colliderDetection.Radius)
			{
				SetAiTarget(target);
			}
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_disposable?.Dispose();
		}

		private void OnDisable()
		{
			_disposable?.Dispose();
		}

		public override void OnCameraChanged(CameraType cameraType)
		{
			
		}

		protected override void OnTakeOffInternal()
		{
			base.OnTakeOffInternal();
			_disposable?.Dispose();
			_colliderDetection.StopScan();
		}

		private void Update()
		{
			if(!IsEquipped)
				return;

			var delta = GetPosition() - MainTransform.position;
			_velocity = Vector3.Lerp(_velocity, GetTargetVelocity(), delta.magnitude * _followMlp);
			var resultTranslation = (_velocity + delta) * (Time.deltaTime * (InAttack ? _attackmMoveSpeed  * 2 : _followSpeed ));

			if (!InAttack)
				resultTranslation.y = Mathf.Clamp(resultTranslation.y, delta.y * .2f, delta.y * 1.25f);
			
			MainTransform.Translate(resultTranslation);
			_model.localPosition += new Vector3(0, Mathf.Sin(Time.time * _amplitude) * _height * Time.deltaTime, 0);
			_model.Rotate(0, 0, Time.deltaTime * (InAttack ? _inactiveRotationSpeed * 2 : _inactiveRotationSpeed ));
		}

		protected override void PlaceCosmetic()
		{
			
		}

		private void OnDrawGizmos()
		{
			if (_originData)
			{
				Gizmos.DrawWireSphere(GetPosition(), 0.5f);
			}
		}

		private Vector3 GetPosition()
		{
			if (InAttack)
				return _currentAiTarget.LookAtPoint;
			return _originData.position + _offset + new Vector3(
				Mathf.Cos(Time.time * _rotationAroundPlayerSpeed) * _radius,
				0, 
				Mathf.Sin(Time.time * _rotationAroundPlayerSpeed) * _radius);
		}
		
		private Vector3 GetTargetVelocity()
		{
			var pos = GetPosition();
			var vel = pos - _lastTargetPosition;
			_lastTargetPosition = pos;
			return vel;
		}
		
		private Quaternion GetRotation()
		{
			return _originData.rotation;
		}
	
		public void SetAiTarget(IAiTarget aiTarget)
		{
			if(Owner == null || Owner.Health.IsDeath)
				return;
			
			HasTarget = true;
			_currentAiTarget = aiTarget;
			AttackByCooldown(destroyCancellationToken).Forget();
		}
		
		public void OnBufferUpdate(Collider[] colliders, in int size)
		{
			if (HasTarget || size <= 0)
				return;
			
			if (size > 1)
			{
				Util.ColliderByDistanceSort.SetPos(transform.position);
				Array.Sort(colliders, 0, size, Util.ColliderByDistanceSort);
			}
			
			for (var i = 0; i < size; i++)
			{
				var col = colliders[i];
				var contains = col.transform.TryGetComponent(out MonoInteractProvider interactProvider);
				
				if (contains && interactProvider.Uid != 0)
				{
					if (EntityRepository.GetEntityById(interactProvider.Uid, out var entityContext) &&
					    entityContext.TryGetAiTarget(out var aiTarget))
					{
						SetAiTarget(aiTarget);
						return;
					}
				}
			}
		}

		private async UniTaskVoid AttackByCooldown(CancellationToken token)
		{
			InAttack = true;
			_lastTargetPosition = GetPosition();
			PlaySound();
			await MoveToTarget(token);
			InAttack = false;
			CastAttack();
			await MoveBack(token);
			await UniTask.Delay(_attackInterval.ToSec(), cancellationToken: token);
			_currentAiTarget = null;
			HasTarget = false;
		}

		private void CastAttack()
		{
			_attackAction.CastAttack(MainTransform.position);
		}

		private async UniTask MoveToTarget(CancellationToken token)
		{
			await UniTask.WaitUntil(() =>
			{ 
				var delta = MainTransform.position - _currentAiTarget.LookAtPoint;
				var distance = delta.magnitude;
				return distance <= 0.35f;
			}, PlayerLoopTiming.LastUpdate, token);
		}
		
		private async UniTask MoveBack(CancellationToken token)
		{
			await UniTask.Delay(1f.ToSec(), cancellationToken: token);
		}
		
		private void PlaySound()
		{
			LucidAudio
				.PlaySE(_clip)
				.SetPosition(MainTransform.position)
				.SetSpatialBlend(1f)
				.SetVolume(_volume);
		}
	}
}