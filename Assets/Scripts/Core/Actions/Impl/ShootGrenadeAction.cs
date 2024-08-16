using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Factory;
using Core.Inventory.Items.Weapon;
using Core.Projectile.Projectile;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ShootGrenadeAction), fileName = nameof(ShootGrenadeAction))]
	public class ShootGrenadeAction : GenericEntityAction<ProjectileWeaponContext>, IActionWithCooldown, IUnlockableAction
	{
		[SerializeField] private string _objectToThrowId;
		[SerializeField] private float _velocity;
		[SerializeField] private float _delay;
		[SerializeField] private AudioClip _shootSound;
		[SerializeField] private bool _awaitAim;

		private ReactiveCommand<float> _cooldown;
		public IObservable<float> OnCooldown => _cooldown;
		public IReactiveProperty<bool> IsUnlocked { get; set; }

		[Inject] private readonly IThrowableFactory _projectileService;
		[Inject] private readonly IRayCastService _rayCastService;
		
		public override void Initialize()
		{
			_cooldown = new ReactiveCommand<float>();
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
		
		public override void OnInputDown()
		{
			if (_awaitAim && CurrentContext.Owner is CharacterContext { IsAimed: false })
				return;
			
			var creationData = GetItemData();
			var projectile =_projectileService.CreateObject(ref creationData);
			projectile.transform.forward = CurrentContext.GetOrigin().forward;
			_cooldown.Execute(_delay);
			
			if (_shootSound)
			{
				LucidAudio
					.PlaySE(_shootSound)
					.SetPosition(CurrentContext.GetOrigin().position)
					.SetVolume(0.1f)
					.SetSpatialBlend(1);
			}
			
		}
		
		protected ThrowableCreationData GetItemData()
		{
			var targetPos = _rayCastService.CurrentHitPoint - _rayCastService.RayPosition;
			var spawnPos = CurrentContext.GetOrigin().position+CurrentContext.GetOrigin().forward;
			targetPos = Vector3.ClampMagnitude(targetPos, _velocity) + _rayCastService.RayPosition;;

			return new ThrowableCreationData(
				spawnPos,
				(targetPos - spawnPos).normalized,
				_velocity,
				CurrentContext.Owner,
				_objectToThrowId
			);
		}
		
	}
}
