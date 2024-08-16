using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using Db.VFXDataDto.Impl;
using GameSettings;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class FlamethrowerWeaponContext : ProjectileWeaponContext
    {
        [TitleGroup("Flamethrower")]
        [SerializeField] protected float _damageDelay = 0.3f;
        [SerializeField] protected SerilizedEffectArgs _effectArgs;
        [SerializeField] protected SerializedDamageArgs _damageArgs;
        protected FlamethrowerOrigin partTrigger;

        [TitleGroup("FX")]
        [SerializeField] protected AudioClip _fireLoopSound;
        [SerializeField] protected float _fireLoopSoundVolume = 0.1f;
        [SerializeField] protected AudioClip _startFireSound;
        [SerializeField] protected float _shootSoundVolume = 0.1f;
        [SerializeField] private string _muzzleName;
        [SerializeField] protected Vector3 _muzzleFPVScale;

        protected VFXContext _vfxContext;
        protected Vector3 _muzzleTPVScale;
        private AudioPlayer _audioPlayer;
        private bool _isReload = false;
        private int _countClip = 0;
        private bool _isShootClick = false;
        private EntityContext _entityContext;

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
                _shouldShootAction.Subscribe(OnInputShoot).AddTo(this);
                _currentClip.Subscribe(OnClipChange).AddTo(this);
                _reloadingAction.Subscribe(OnReloding).AddTo(this);
                _entityContext = GetOrigin().GetComponentInChildren<FlamethrowerOrigin>().CurrentContext;
            }
        }

        private void OnReloding(bool reloading)
        {
            _isReload = reloading;
            if (_isReload)
                return;

            if (_isShootClick && _countClip > 0)
            {
                StartPlaySound();
                ActivateFire();
                return;
            }

            DeactivateFire();
            StopPlaySound();
        }

        private void OnClipChange(int value)
        {
            _countClip = value;
            if (_countClip > 0)
                return;

            DeactivateFire();
            StopPlaySound();
        }

        private void OnInputShoot(bool isShoot)
        {
            _isShootClick = isShoot;
            if (!isShoot || _isReload || _countClip <= 0)
            {
                DeactivateFire();
                StopPlaySound();
                return;
            }

            ActivateFire();
            StartPlaySound();
        }

        private void DeactivateFire()
        {
            partTrigger?.Deactivate();
        }

        private void ActivateFire()
        {
            partTrigger = GetOrigin().GetComponentInChildren<FlamethrowerOrigin>();
            partTrigger.Init(_effectArgs, _damageArgs, _damageDelay);
            partTrigger.CurrentContext = _entityContext;
            partTrigger?.Activate();
        }

        private void StartPlaySound()
        {
            LucidAudio
                 .PlaySE(_startFireSound)
                  .SetPosition(GetOrigin().transform.position)
                  .SetVolume(_shootSoundVolume)
                  .SetSpatialBlend(1);

            StopPlaySound();
            _audioPlayer = LucidAudio
                .PlaySE(_fireLoopSound)
                .SetSpatialBlend(1f)
                .SetLoop(true)
                .SetVolume(_fireLoopSoundVolume)
                .SetPosition(GetOrigin().transform.position);
        }

        private void StopPlaySound()
        {
            if (_audioPlayer is null)
                return;

            _audioPlayer.Stop();
            _audioPlayer = null;
        }

        private void LateUpdate()
        {
            _audioPlayer?.SetPosition(GetOrigin().transform.position);
        }

        public override void Shoot()
        {
            OnShoot();

            if (!_infinityClip)
                _currentClip.Value--;
        }

        protected override void OnShoot()
        {
            if (ItemAnimator)
            {
                ItemAnimator.PlayAnim("Shoot");
            }
        }
    }
}