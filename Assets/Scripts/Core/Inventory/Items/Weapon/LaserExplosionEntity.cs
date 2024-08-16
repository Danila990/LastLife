using System;
using Core.Entity.Characters;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
	public class LaserExplosionEntity : ExplosionEntity
	{
		[SerializeField, TabGroup("SceneRefs")] private LineRenderer _laserLine;
		[SerializeField, TabGroup("SceneRefs")] private SnapObject _snapObject;
		[SerializeField, TabGroup("SceneRefs")] private Transform _origin;
		[SerializeField, TabGroup("Parameters")] private float _armedTime;
		[SerializeField, TabGroup("Parameters")] private float _rayLength = 1000f;
		[SerializeField, TabGroup("Sfx")] private AudioClip _activationSound;
		[SerializeField, TabGroup("Sfx")] private float _volume;

		private bool _isArmed;
		private IDisposable _disposable;
		private Vector3 _endPoint;
		private float _rayDistance;

		protected override void OnUpdate() { }
		
		private void FixedUpdate()
		{
			if(!_isArmed)
				return;

			if (CheckTrigger())
			{
				_isArmed = false;
				ExplosionVFX();
			}
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			ExplosionVFX();
		}
		
		protected override void OnCollisionEnter(Collision other) { }


		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			_laserLine.enabled = false;
			_disposable = _snapObject.OnAttached.Subscribe(_ => OnAttach());
		}

		protected void OnDestroy()
		{
			_disposable?.Dispose();
		}

		private bool CheckTrigger()
		{
			var triggered = Physics.Raycast(_origin.position, _origin.forward, out var hit, _rayLength);
			
			var currentLenght = 0f;
			
			currentLenght = triggered
				? hit.distance
				: _rayLength;

			return Mathf.Abs(currentLenght - _rayDistance) > 0.1f;
		}
		
		private void OnAttach()
			=> ArmAsync().Forget();

		private async UniTaskVoid ArmAsync()
		{
			await UniTask.Delay(_armedTime.ToSec(), cancellationToken: destroyCancellationToken);

			if (AudioService.TryPlayQueueSound(_activationSound, Uid.ToString(), _activationSound.length, out var player))
			{
				player
					.SetPosition(transform.position)
					.SetVolume(_volume)
					.SetSpatialBlend(1);
			}
			
			var triggered = Physics.Raycast(_origin.position, _origin.forward, out var hit, _rayLength);

			if (!triggered)
			{
				_endPoint = _origin.forward * _rayLength;
				_rayDistance = _rayLength;
			}
			else
			{
				_endPoint = hit.point;
				_rayDistance = hit.distance;
			}
			
			
			_laserLine.SetPosition(1, _origin.InverseTransformPoint(_endPoint));
			_laserLine.enabled = true;
			_isArmed = true;
		}

	}
}
