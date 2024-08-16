using System;
using System.Collections.Generic;
using System.Linq;
using Core.Boosts;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.EntityUpgrade;
using Core.Factory.DataObjects;
using Core.Inventory.Origins;
using Core.Projectile;
using Core.Projectile.Projectile;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Inventory.Items.Weapon
{

    public abstract class ProjectileWeaponContext : WeaponContext, IUpgradableEntity
    {
        [TitleGroup("Main")]
        [field:SerializeField] public ProjectileWeaponData WeaponData { get; set; }
        [SerializeField] protected Vector3 _spawnOffset;
        [SerializeField] protected float _cooldown;
        [BoxGroup("Bullet")] [SerializeField] protected float _inaccuracy;
        [BoxGroup("Bullet")] [SerializeField] protected int _shootCount = 1;
        [BoxGroup("Bullet")] [SerializeField] private float _bulletLifetime = 1;
        [BoxGroup("Bullet")] [SerializeField] private float _velocityRandomizeMlp;
        
        [BoxGroup("Reload")] [SerializeField] protected bool _infinityClip;
        [BoxGroup("Reload")] [SerializeField, HideIf("_infinityClip")] private float _reloadTime;
        [BoxGroup("Reload")] [SerializeField, HideIf("_infinityClip")] private int _clip;
        [BoxGroup("Reload")] [SerializeField, HideIf("_infinityClip")] private AudioClip _reloadSound;

        [Inject] protected readonly IProjectileService ProjectileService;
        private UpgradeableParameters _upgradeableParameters;
        private IDisposable _reloading;
        protected ReactiveProperty<bool> _reloadingAction;
        protected ReactiveProperty<int> _currentClip;
        protected ReactiveProperty<bool> _shouldShootAction;
        private ReactiveCommand<int> _onClipChanged;
        
        private float _currentCooldown = 0;
        private bool _shouldShoot;
        protected bool _isDebug = true;
        
        protected BaseOriginProvider ShootingOrigin;
        protected CharacterContext CharacterContext;
        private Vector3 SpawnPos => GetOrigin().position + GetOrigin().rotation * _spawnOffset;
        public virtual float CoolDown => _cooldown;
        public IReactiveProperty<bool> Reloading => _reloadingAction;
        public IReactiveProperty<int> CurrentClip => _currentClip;
        public IReactiveCommand<int> OnClipChanged => _onClipChanged;
        public float ReloadTime => _reloadTime;
        public int ClipSize => (int)GetValue(WeaponConsts.CAPACITY);
        public ref UpgradeableParameters Parameters => ref _upgradeableParameters;
        public virtual bool ShouldReload => _reloading == null && (_currentClip.Value <= 0 || _currentClip.Value < ClipSize) && !_infinityClip;
        public virtual bool IsClipEmpty => _reloading == null && _currentClip.Value <= 0 && !_infinityClip;

        protected override void OnCreated(IObjectResolver resolver)
        {
            base.OnCreated(resolver);
            if (!_infinityClip)
                CharacterContext = (CharacterContext)Owner;
        }

        public void OnUpgrade()
        {
            OnClipChanged.Execute(_currentClip.Value);
        }
        
        public void OnUpgradesInit()
        {
            ForceReload();
        }
        
        public void FixedUpdate()
        {
            _currentCooldown -= Time.fixedDeltaTime;
            if(!_infinityClip && _currentClip.Value <= 0 || _reloading != null )
                return;
            
            if (_shouldShoot)
            {
                ShootWithCd();
            }
        }

        public override void ItemInit(IOriginProxy inventory)
        {
            base.ItemInit(inventory);
            ResetUpgrades();
            ShootingOrigin = inventory.Origin;
            
            _reloadingAction = new ReactiveProperty<bool>().AddTo(this);
            _currentClip = new ReactiveProperty<int>().AddTo(this);
            _onClipChanged = new ReactiveCommand<int>().AddTo(this);
            _shouldShootAction = new ReactiveProperty<bool>().AddTo(this);
            ForceReload();

            if (ItemAnimator is WeaponItemAnimator weaponItemAnimator)
            {
                weaponItemAnimator.Initialize(CharacterContext, this, _reloadSound, 1f, 1f / _reloadTime);
            }
        }

        public void ResetUpgrades()
        {
            var dict = new Dictionary<string, float>()
            {
                { WeaponConsts.CAPACITY, _clip },
                { WeaponConsts.DAMAGE, 0 },
                { WeaponConsts.DISTANCE, 0},
                { WeaponConsts.ACCURACY, 0},
                { WeaponConsts.FALLOFF, 1},
                { WeaponConsts.SPIN, 0},
                { WeaponConsts.FIRE_RATE, 0 }
            };
            
            _upgradeableParameters = new UpgradeableParameters(dict);
        }

        public float GetValue(string key) => _upgradeableParameters.GetValue(key);

        public void StartReload()
        {
            if(_reloading != null || _infinityClip)
                return;

            _reloadingAction.Value = true;
            _reloading = Observable
                .Timer(ReloadTime.ToSec(), Scheduler.MainThread)
                .Finally(() =>
                {
                    _reloading?.Dispose();
                    _reloading = null;
                })
                .TakeUntilDestroy(this)
                .Subscribe(_ =>
                {
                    ForceReload();
                });
        }

        public void StopReload()
        {
            _reloading?.Dispose();
            _reloadingAction.Value = false;
        }

        private void ForceReload()
        {
            _currentClip.Value = ClipSize;
            _reloading?.Dispose();
            _reloadingAction.Value = false;
        }
        
        public void SetShootStatus(bool pressed)
        {
            _shouldShoot = pressed;
            _shouldShootAction.Value = _shouldShoot;
        }

        public Transform GetOrigin() => ShootingOrigin.GetOrigin("aim", this);

        public virtual void ShootWithCd()
        {
            if (_currentCooldown <= 0)
            {
                Shoot();                      //Upgrade: FireRate
                _currentCooldown = CoolDown - GetValue(WeaponConsts.FIRE_RATE);
            }
        }
        
        public virtual void Shoot()
        {
#if UNITY_EDITOR
            if (_isDebug)
            {
                GetOrigin();
                var forward = GetOrigin().forward;
                var position = GetOrigin().position;
                
                var firstCircle = position + Quaternion.LookRotation(forward) * new Vector3(0, 0, 1);
                Util.DrawCircle(firstCircle, Quaternion.LookRotation(forward), _inaccuracy, 12, Color.cyan, 1);
                var nextCirclePos = position + Quaternion.LookRotation(forward) * new Vector3(0, 0, 5);
                Util.DrawCircle(nextCirclePos, Quaternion.LookRotation(forward), _inaccuracy * 5, 12, Color.cyan, 1);
            }
#endif
            
            for (var i = 0; i < _shootCount; i++)
            {
                var origin = GetOrigin();
                var inaccuracy = GetInaccuracy(origin);
                var dir = GetDir(origin);
               
                var link = CreateProjectile(origin.position, dir, inaccuracy);
                link.Controller.Mass /= GetValue(WeaponConsts.FALLOFF);
            }
            OnShoot();
            
            if(!_infinityClip)
                _currentClip.Value--;
        }
        
        protected ProjectileLink CreateProjectile(Vector3 pos, Vector3 dir, Vector3 inaccuracy = default)
        {
            Debug.DrawRay(pos, dir, Color.blue, 1f);
            Debug.DrawRay(pos, dir + inaccuracy, Color.red, 1f);
            
            ProjectileService.CreateProjectile(
                new ProjectileCreationData(
                    pos,
                    dir + inaccuracy,
                    GetVelocity(),
                    Owner,
                    WeaponData.ProjectileKey,
                    _bulletLifetime + GetValue(WeaponConsts.DISTANCE)
                ), out var link);

            var additionalDamage = 0f;
            
            if(!_infinityClip)
                CharacterContext.Adapter.StatsProvider?.Stats.GetValue(StatType.AllDamage, out additionalDamage);

            link.View.AdditionalDamage = GetValue(WeaponConsts.DAMAGE) + additionalDamage;
            return link;
        }
        
        protected Vector3 GetDir(Transform origin)
        {
            return origin.forward;
        }
        
        private float GetVelocity()
        {
            return WeaponData.Velocity + Random.Range(-_velocityRandomizeMlp, _velocityRandomizeMlp);
        }

        protected virtual Vector3 GetInaccuracy(Transform origin)
        {
            var rotation = Quaternion.LookRotation(origin.forward);
            return rotation * Random.insideUnitCircle * (_inaccuracy + GetValue(WeaponConsts.ACCURACY));
        }

        protected abstract void OnShoot();

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            
            if (ShootingOrigin)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(SpawnPos, 0.15f);
                Util.ForGizmo(GetOrigin().position, GetOrigin().forward);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Vector3.zero, 0.15f);
            }
        }
#endif
        
    }


    [Serializable]
    public class ProjectileWeaponData
    {
        public float Velocity;
        
        [InlineButton("Refresh")]
        public string ProjectileKey;

#if UNITY_EDITOR
        [InlineEditor]
        [HideLabel]
        [ReadOnly]
        [ShowInInspector]
        private EntityContext _projectile;

        public void Refresh()
        {
            _projectile = FactoryData.EditorInstance.Objects.FirstOrDefault(o => o.Key == ProjectileKey).Object.GetComponent<ProjectileEntity>();
        }
#endif
    }
}