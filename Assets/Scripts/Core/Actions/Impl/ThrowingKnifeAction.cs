using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Inventory.Items.Weapon;
using Core.Projectile.Projectile;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ThrowingKnifeAction), fileName = nameof(ThrowingKnifeAction))]
	public class ThrowingKnifeAction : GenericEntityAction<ProjectileWeaponContext>, IActionWithCooldown, IUnlockableAction
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Projectile)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField] private string _objectToThrowId;
		[SerializeField] private float _velocity;
		[SerializeField] private float _delay;
		[SerializeField] private float _lifeTime = 10f;
		[SerializeField] private AudioClip _shootSound;
		[SerializeField] private bool _awaitAim;
		
		private ReactiveCommand<float> _cooldown;
		public IObservable<float> OnCooldown => _cooldown;
		public IReactiveProperty<bool> IsUnlocked { get; set; }

		[Inject] private readonly IProjectileService _projectileService;
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
			
			var origin = CurrentContext.GetOrigin();
			var endPoint = _rayCastService.CurrentHitPoint - _rayCastService.RayPosition;
			endPoint = Vector3.ClampMagnitude(endPoint, _velocity * _lifeTime) + _rayCastService.RayPosition;
			
			var creationData = new ProjectileCreationData(
				origin.position,
				(endPoint - origin.position).normalized,
				_velocity, 
				CurrentContext.Owner,
				_objectToThrowId,
				_lifeTime);
			_projectileService.CreateProjectile(creationData);
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
	}
}