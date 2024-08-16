using System;
using AnnulusGames.LucidTools.Audio;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Factory;
using Core.Inventory.Items.Weapon;
using Core.Projectile.Projectile;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ThrowingGranadeAction), fileName = nameof(ThrowingGranadeAction))]
	public class ThrowingGranadeAction : GenericEntityAction<ProjectileWeaponContext>, IActionWithCooldown, IUnlockableAction
	{
		[SerializeField] private string _objectToThrowId;
		[SerializeField] private float _velocity;
		[SerializeField] private float _delay;
		[SerializeField] private AudioClip _shootSound;
		[SerializeField] private float _volume = 0.1f;
		[SerializeField] private AnimationClip _throwTpvClip;
		[SerializeField] private bool _awaitAim;
		
		private ReactiveCommand<float> _cooldown;
		public IObservable<float> OnCooldown => _cooldown;
		public IReactiveProperty<bool> IsUnlocked { get; set; }

		[Inject] private readonly IThrowableFactory _projectileService;
		[Inject] private readonly IRayCastService _rayCastService;
		[Inject] private readonly ICameraService _cameraService;
		private bool _isThrowing;

		public override void Initialize()
		{
			_cooldown = new ReactiveCommand<float>();
			_isThrowing = false;
		}

		public override void Dispose()
		{
			_cooldown?.Dispose();
		}

		public override void OnDeselect()
		{
		}
		
		public override void OnInput(bool state)
		{
			
		}
		
		public override void OnInputUp()
		{
			
		}

		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);
			_isThrowing = false;
		}

		public override void OnInputDown()
		{
			if (_awaitAim && CurrentContext.Owner is CharacterContext { IsAimed: false })
				return;
			
			if (_cameraService.IsThirdPerson)
			{
				ThrowThpObject().Forget();
			}
			else
			{
				Throw();
			}
		}
		
		private void Throw()
		{
			var creationData = GetItemData();
			_projectileService.CreateObject(ref creationData);
			_cooldown.Execute(_delay);
			
			if (_shootSound)
			{
				LucidAudio
					.PlaySE(_shootSound)
					.SetPosition(CurrentContext.GetOrigin().position)
					.SetVolume(_volume)
					.SetSpatialBlend(1);
			}
		}
		
		private async UniTaskVoid ThrowThpObject()
		{
			if(_isThrowing) 
				return;
            
			_isThrowing = true;
			var res = await ((CharacterContext)CurrentContext.Owner)!.CharacterAnimator.PlayAction(_throwTpvClip, .35f, 1, 2f);
			_isThrowing = false;
			if(!res) 
				return;
			await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: CurrentContext.destroyCancellationToken);
			Throw();
		}

		protected ThrowableCreationData GetItemData()
		{
			var targetPos = _rayCastService.CurrentHitPoint - _rayCastService.RayPosition;
			var spawnPos = CurrentContext.GetOrigin().position + CurrentContext.GetOrigin().forward;
			targetPos = Vector3.ClampMagnitude(targetPos, 25) + _rayCastService.RayPosition;

			
			Util.DrawSphere(targetPos, Quaternion.identity, 2,Color.yellow,1);
            
			var res = Calc(spawnPos, targetPos);
			return new ThrowableCreationData(
				spawnPos,
				res.normalized,
				res.magnitude,
				CurrentContext.Owner,
				_objectToThrowId
			);
		}

		private static Vector3 Calc(Vector3 startPos, Vector3 targetPos)
		{
			var maxH = Mathf.Max(startPos.y, targetPos.y) + 1;
			var h1 = maxH - startPos.y;
			var h2 = maxH - targetPos.y;
			var g = 9.81f;
			var velY = Mathf.Sqrt(2 * h1 * g);
			var t1 = velY / g;
			var t2 = Mathf.Sqrt(2 * h2 / g);
			var t = t1 + t2;
			var delta = targetPos - startPos;
			delta.y = 0;
			var velXZ = delta / t;
			return velY  * Vector3.up + velXZ;
		}
	}

}