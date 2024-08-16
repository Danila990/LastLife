using System;
using System.Threading;
using Core.HealthSystem;
using Core.Services.Experience;
using Cysharp.Threading.Tasks;
using Installer;
using LitMotion;
using LitMotion.Extensions;
using MessagePipe;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class DummyDamagable : EnviromentProjectileInteraction, IInjectableTag
	{
		[Inject] private readonly IPublisher<ExperienceMessage> _experiencePublisher;
		[SerializeField] private Transform _fallingTransform;
		[SerializeField] private Vector3 _fallRotate;
		private Quaternion _initRot;
		private Quaternion _targetRot;
		private bool _isFall;
		private MotionHandle _backFromFall;

		private void Start()
		{
			if (_experiencePublisher is null)
			{
				Debug.Log($"Dummy {name} not injected");
			}
			_initRot = _fallingTransform.localRotation;
			_targetRot = Quaternion.Euler(_fallRotate);
		}

		private void Fall()
		{
			_isFall = true;
			FallAsync(destroyCancellationToken).Forget();
		}

		private async UniTaskVoid FallAsync(CancellationToken token)
		{
			await LMotion
				.Create(_fallingTransform.localRotation, _targetRot, 0.3f)
				.BindToLocalRotation(_fallingTransform)
				.ToUniTask(token);
			
			await UniTask.Delay(1500, cancellationToken: token);


			_backFromFall = LMotion
				.Create(_fallingTransform.localRotation, _initRot, 0.3f)
				.BindToLocalRotation(_fallingTransform);
			
			_isFall = false;
		}

		public override void OnHit(ref DamageArgs args, ref InteractionCallMeta meta)
		{
			if (Vector3.Dot(meta.Normal, _fallingTransform.forward) <= 0.6f)
				return;
			
			if (_isFall)
				return;
			
			_backFromFall.IsActiveCancel();
			_experiencePublisher?.Publish(new ExperienceMessage(5, this, args.DamageSource, transform.position + Vector3.up * 2));
			Fall();
		}

		private void OnDisable()
		{
			_backFromFall.IsActiveCancel();
		}
	}
}