using AnnulusGames.LucidTools.Audio;
using Cysharp.Threading.Tasks;
using Db.VFXDataDto.Impl;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
	public class ProjectileWithDelay : ProjectileWeaponContext
	{
		[TitleGroup("DelayData")]
		[SerializeField] private float _delayTime = 0.5f;
		
		[TitleGroup("FX")]
		[SerializeField] private AudioClip ShootSound;
		[SerializeField] private float _shootSoundVolume = 0.1f;
		[SerializeField] private string _muzzleName;
		[TitleGroup("DelayFX")]
		[SerializeField] private AudioClip _delaySound;
		[SerializeField] private float _delaySoundVolume = 0.1f;
		[SerializeField] private string _delayFXName;

		public override float CoolDown => _delayTime + _cooldown;

		private VFXContext _vfxContext;
		private VFXContext _delayVfxContext;
		
		public override void ItemInit(IOriginProxy inventory)
		{
			base.ItemInit(inventory);
			var origin = ShootingOrigin.GetStaticOrigin();
			if (!string.IsNullOrEmpty(_muzzleName) && VFXFactory.TryGetParticle(_muzzleName, out _vfxContext))
			{
				_vfxContext.Attach(origin.position, origin.forward, null);
				_vfxContext.transform.SetParent(origin);
				VFXFactory.ReleaseOnDestroy(_muzzleName, _vfxContext, origin);
			}
			
			if (!string.IsNullOrEmpty(_delayFXName) && VFXFactory.TryGetParticle(_delayFXName, out _delayVfxContext))
			{
				_delayVfxContext.Attach(origin.position, origin.forward, null);
				_delayVfxContext.transform.SetParent(origin);
				VFXFactory.ReleaseOnDestroy(_delayFXName, _delayVfxContext, origin);
			}
		}

		public override void Shoot()
		{
			ShootTask().Forget();
		}

		private async UniTaskVoid ShootTask()
		{
			if (_delayVfxContext)
			{
				var origin = GetOrigin();
				_delayVfxContext.transform.SetParent(origin);
				_delayVfxContext.ParticleSystem.transform.SetPositionAndRotation(origin.position, origin.rotation);
				_delayVfxContext.Play();
			}

			if (_delaySound)
			{
				LucidAudio
					.PlaySE(_delaySound)
					.SetPosition(ShootingOrigin.GetStaticOrigin().position)
					.SetVolume(_delaySoundVolume)
					.SetSpatialBlend(1);
			}
			await UniTask.Delay(_cooldown.ToSec(), cancellationToken: destroyCancellationToken);
			base.Shoot();
		}

		protected override void OnShoot()
		{
			if (_vfxContext)
			{
				var origin = GetOrigin();
				_vfxContext.transform.SetParent(origin);
				_vfxContext.ParticleSystem.transform.SetPositionAndRotation(origin.position,origin.rotation);
				_vfxContext.Play();
			}

			if (ShootSound)
			{
				LucidAudio
					.PlaySE(ShootSound)
					.SetPosition(ShootingOrigin.GetStaticOrigin().position)
					.SetVolume(_shootSoundVolume)
					.SetSpatialBlend(1);
			}
			
			
			if (ItemAnimator)
			{
				ItemAnimator.PlayAnim("Shoot");
			}
		}
	}
}