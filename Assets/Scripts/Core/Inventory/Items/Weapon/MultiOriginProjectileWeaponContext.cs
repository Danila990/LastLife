using AnnulusGames.LucidTools.Audio;
using Db.VFXDataDto.Impl;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
	public class MultiOriginProjectileWeaponContext : ProjectileWeaponContext
	{
		[TitleGroup("FX")]
		[SerializeField] private AudioClip ShootSound;
		[SerializeField] private float _shootSoundVolume = 0.1f;
		[SerializeField] private string _muzzleName;
		[SerializeField] private Vector3 _muzzleFPVScale;
		[SerializeField] private string[] _originNames;
		private VFXContext _vfxContext;
		
		public override void Shoot()
		{
			var shootCount = Random.Range(1, _originNames.Length + 1);
			_originNames.Shuffle();
			for (int i = 0; i < shootCount; i++)
			{
				var origin = ShootingOrigin.GetOrigin(_originNames[i]);
				var inaccuracy = GetInaccuracy(origin);
				var dir = GetDir(origin);
				CreateProjectile(origin.position, dir, inaccuracy);

				if (!string.IsNullOrEmpty(_muzzleName) && VFXFactory.TryGetParticle(_muzzleName, out _vfxContext))
				{
					_vfxContext.transform.SetParent(origin);
					_vfxContext.ParticleSystem.transform.SetPositionAndRotation(origin.position, origin.rotation);
					_vfxContext.ParticleSystem.transform.localScale = IsTpvMode ? Vector3.one : _muzzleFPVScale;  
					_vfxContext.Play();
					VFXFactory.ReleaseOnDestroyAndForget(_muzzleName, _vfxContext, origin);
				}
			}
			
			if (ShootSound)
			{
				LucidAudio
					.PlaySE(ShootSound)
					.SetPosition(ShootingOrigin.GetStaticOrigin().position)
					.SetVolume(_shootSoundVolume)
					.SetSpatialBlend(1);
			}
			//_animationBehaviour.PlayAnim("Shoot");
		}

		protected override void OnShoot()
		{
		}
	}
}