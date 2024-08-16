using Core.Inventory.Items.Weapon;
using Core.Projectile.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class ProjectileStaticWeaponContext : AbstractStaticWeaponContext
	{
		[field:SerializeField, BoxGroup("Weapon Settings")] public ProjectileWeaponData WeaponData { get; set; }
		[SerializeField, BoxGroup("Weapon Settings")] protected float _cooldown;
		[SerializeField, BoxGroup("Weapon Settings")] protected Transform _shootOrigin;

		[BoxGroup("Bullet")] [SerializeField] private float _inaccuracy;
		[BoxGroup("Bullet")] [SerializeField] private int _shootCount = 1;
		[BoxGroup("Bullet")] [SerializeField] private float _bulletLifetime = 1;
		[BoxGroup("Bullet")] [SerializeField] private float _velocityRandomizeMlp;
		[BoxGroup("VFX")] [SerializeField] private AudioClip _shootSound;
		[Inject] protected readonly IProjectileService ProjectileService;
		private float _currentCooldown;
		private bool _shouldShoot;

		public float CoolDown => _cooldown;

		public void SetShootStatus(bool b)
		{
			_shouldShoot = b;
		}
		
		public override void TickOnActive()
		{
			_currentCooldown -= Time.fixedDeltaTime;
			
			if(HasQuantity && CurrentQuantity.Value <= 0) 
				return;
			
			if (_shouldShoot)
			{
				ShootWithCd();
			}
		}
		
		public virtual void ShootWithCd()
        {
            if (_currentCooldown <= 0)
            {
                Shoot();
                _currentCooldown = CoolDown;
            }
        }

		public virtual void Shoot()
        {
            for (var i = 0; i < _shootCount; i++)
            {
                var origin = GetOrigin();
                var inaccuracy = GetInaccuracy(origin);
                var dir = origin.forward;
                CreateProjectile(origin.position, dir, inaccuracy);
            }
            
            AudioService
	            .PlayNonQueue(_shootSound)
	            .SetPosition(_shootOrigin.position)
	            .SetSpatialBlend(1);
            
            //OnShoot();
            if(!HasQuantity) 
	            return;
            
            CurrentQuantity.Value--;
        }
        
        
        protected void CreateProjectile(Vector3 pos, Vector3 dir, Vector3 inaccuracy = default)
        {
            Debug.DrawRay(pos, dir, Color.blue, 1f);
            Debug.DrawRay(pos, dir + inaccuracy, Color.red, 1f);
            ProjectileService.CreateProjectile(
                new ProjectileCreationData(
                    pos,
                    dir + inaccuracy,
                    GetVelocity(),
                    CharacterAdapter.Entity,
                    WeaponData.ProjectileKey,
                    _bulletLifetime
                ));
        }
        
        private float GetVelocity()
        {
            return WeaponData.Velocity + Random.Range(-_velocityRandomizeMlp, _velocityRandomizeMlp);
        }

        protected virtual Vector3 GetInaccuracy(Transform origin)
        {
            var rotation = Quaternion.LookRotation(origin.forward);
            return rotation* Random.insideUnitCircle * _inaccuracy;
        }

        public Transform GetOrigin()
        {
	        return _shootOrigin;
        }
	}
}