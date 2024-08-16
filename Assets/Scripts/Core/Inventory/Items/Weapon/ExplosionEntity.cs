using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Core.Projectile.Projectile;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Inventory.Items.Weapon
{

    public class ExplosionEntity : ThrowableEntity, IInteractorVisiter, IExplosionVisiter
    {
        [SerializeField] protected float _explosionDelay;
        [SerializeField] private float _explosionRadius;
        [SerializeField] private SerializedDamageArgs _explosionArgs;
        [SerializeField] private string _VFXKey;
        [SerializeField] private AudioClip _explosionSound;
        [SerializeField] private bool _damageSourceIsOwner;
        [SerializeField] private bool _damageSelf;
        [SerializeField] private bool _spawnDebris;
        [SerializeField] private bool _anyImpactExpl;
        
        [ShowIf("SpawnDebris")] [SerializeField] private ProjectileWeaponData _debrisData;
        [ShowIf("SpawnDebris")] [SerializeField] private int _debrisCount;
        
        [Inject] private readonly IProjectileService _projectileService;
        [Inject] private readonly IOverlapInteractionService _overlapInteractionService;
        
        protected float CurrentDelay;
        protected bool IsExploded;
        
        public bool SpawnDebris => _spawnDebris;
        private uint SelfUid => _damageSelf ? 0 : Owner.Uid;
        private EntityContext SelfOwner => _damageSourceIsOwner ? Owner : this;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }

        protected override void OnCreated(IObjectResolver resolver)
        {
            CurrentDelay = _explosionDelay;
        }
        
        private void Update()
        {
            if(IsExploded) 
                return;
            OnUpdate();
        }

        protected virtual void OnCollisionEnter(Collision other)
        {
            if (_anyImpactExpl)
            {
                ExplosionVFX();
                return;
            }
            if (Layers.ContainsLayer(LayerMasks.OrganicLayers, other.collider.gameObject.layer))
            {
                ExplosionVFX();
            }
        }

        protected virtual void OnUpdate()
        {
            CurrentDelay -= Time.deltaTime;
            if (CurrentDelay <= 0)
            {
                ExplosionVFX();
            }
        }

        public virtual void ExplosionVFX()
        {
            if(IsExploded) 
                return;

            VFXFactory.CreateAndForget(_VFXKey, transform.position);
            if (_explosionSound)
            {
                LucidAudio
                    .PlaySE(_explosionSound)
                    .SetPosition(transform.position)
                    .SetSpatialBlend(1f);
            }
            Explosion();
        }
        
        protected virtual void Explosion()
        {
            if(IsExploded)
                return;
            
            IsExploded = true;
            var pos = transform.position;
            _overlapInteractionService.OverlapSphere(this, pos, _explosionRadius, SelfUid);
            if (_spawnDebris)
            {
                SpawnDebrisProjectiles(pos);
            }
            Disable().Forget();
        }

        private async UniTaskVoid Disable()
        {
            await UniTask.NextFrame(destroyCancellationToken);
            Destroy(gameObject);
        }

        protected virtual void SpawnDebrisProjectiles(Vector3 pos)
        {
            for (var i = 0; i < _debrisCount; i++)
            {
                var rndDir = Random.insideUnitSphere;
                if (Physics.Raycast(transform.position, rndDir, 1, LayerMasks.Environment))
                {
                    rndDir = -rndDir;
                }
                _projectileService.CreateProjectile(new ProjectileCreationData(
                    pos,
                    rndDir,
                    _debrisData.Velocity,
                    SelfOwner,
                    _debrisData.ProjectileKey,
                    3f
                ));
            }
        }

        public virtual InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta)
        {
            var args = _explosionArgs.GetArgs(SelfOwner);
            args.MetaDamageSource = this;
            var delta = (meta.Point - MainTransform.position).normalized;
            damage.HandleExplosion(ref args, meta.Point, delta, delta);
            return StaticInteractionResultMeta.InteractedBlocked;
        }
        protected virtual void OnAccept(EntityDamagable damage, Vector3 metaPoint) { }
        public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
        public virtual InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

        public virtual InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
        {
            var args = _explosionArgs.GetArgs(SelfOwner);
            var delta = (meta.Point - MainTransform.position).normalized;
            damagable.DoDamageExplosion(ref args, meta.Point, delta, delta);
            OnAccept(damagable, meta.Point);

            if (damagable.DontCashDamagedUIDs)
            {
                return new InteractionResultMeta { Interacted = true, HitBlock = true, DontCache = true };
            }
            return StaticInteractionResultMeta.InteractedBlocked;
        }
    }
    public interface IExplosionVisiter
    {
    }
}