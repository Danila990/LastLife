using AnnulusGames.LucidTools.Audio;
using Db.VFXDataDto.Impl;
using GameSettings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class SimpleProjectileWeaponContext : ProjectileWeaponContext
    {
        [TitleGroup("FX")]
        [SerializeField]
        protected AudioClip ShootSound;
        [SerializeField] protected float _shootSoundVolume = 0.1f;
        [SerializeField] private string _muzzleName;
        [SerializeField] protected Vector3 _muzzleFPVScale;
        protected VFXContext _vfxContext;
        protected Vector3 _muzzleTPVScale;
        
        public override void ItemInit(IOriginProxy inventory)
        {
            base.ItemInit(inventory);
            if (!string.IsNullOrEmpty(_muzzleName) && VFXFactory.TryGetParticle(_muzzleName, out _vfxContext))
            {
                var staticOrigin = ShootingOrigin.GetStaticOrigin();
                _muzzleTPVScale = _vfxContext.ParticleSystem.transform.localScale;
                _vfxContext.Attach(staticOrigin.position, staticOrigin.forward, null);
                _vfxContext.transform.SetParent(staticOrigin);
                VFXFactory.ReleaseOnDestroy(_muzzleName, _vfxContext, staticOrigin);
            }
        }

        protected override void OnShoot()
        {
            if (_vfxContext)
            {
                var origin = GetOrigin();
                _vfxContext.transform.SetParent(origin);
                _vfxContext.ParticleSystem.transform.SetPositionAndRotation(origin.position, origin.rotation);
                _vfxContext.ParticleSystem.transform.localScale = IsTpvMode ? _muzzleTPVScale : _muzzleFPVScale;
                if (!GameSetting.ViolenceStatus)
                {
                    _vfxContext.Play();
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
            

            if (ItemAnimator)
            {
                ItemAnimator.PlayAnim("Shoot");
            }
        }
    }
}