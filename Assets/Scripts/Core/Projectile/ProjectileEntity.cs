using System;
using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Core.Projectile.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;
using uPools;
using Utils;

namespace Core.Projectile
{
    public abstract class ProjectileEntity : EntityContext, IInteractorVisiter, IOwnedEntity
    {
        protected IObjectPool<ProjectileEntity> Pool;
        public float BulletSphereSize;
        public float BulletWeight;
        public float Volume;
        public SerializedDamageArgs DamageArgs;
        [TitleGroup("Delay")]
        public float ReleaseDelay;
        public bool DontAffectDistance;
        [ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
        public string ImpactOverrideFX;
        
        [ShowInInspector, NonSerialized, ReadOnly] 
        public bool IsReleased;
        protected LastInteractionType LastInteraction;

        private EntityContext _owner;
        protected ProjectileLink _currentLink;

        public ProjectileLink CurrentLink => _currentLink;
        public EntityContext Owner
        {
            get => _owner;
            set => SetOwner(value);
        }
        public float AdditionalDamage { get; set; }

        public virtual void ProjectileUpdate(){}
        public abstract void OnHitTarget(IReadOnlyList<RaycastHit> targets, int hitCount, EntityContext source, out bool isBlocking, out RaycastHit hit);

        public abstract void Release();
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (BulletSphereSize > 0)
            {
                Gizmos.DrawWireSphere(transform.position, BulletSphereSize);
            }
        }
#endif
        
        public virtual void OnRent()
        {
            AdditionalDamage = 0;
            LastInteraction = LastInteractionType.None;
        }

        public virtual void OnCollision(IProjectileService service)
        {
            service.ReleaseProjectile(_currentLink);
        }

        public void SetPool(IObjectPool<ProjectileEntity> pool)
        {
            Pool = pool;
        }

        public virtual void SetLink(ProjectileLink link)
        {
            _currentLink = link;
        }

        protected virtual void SetOwner(EntityContext context)
        {
            _owner = context;
        }
        
        public virtual void HandleHit(
            RaycastHit hit,
            out bool isBlocking)
        {
            isBlocking = false;
            var res = VisiterUtils.RayVisit(hit,this);
            isBlocking = res.HitBlock;
        }
        
#region Interaction
        public virtual InteractionResultMeta Accept(EntityDamagable damageInteraction, ref InteractionCallMeta meta)
        {
            if (damageInteraction.TargetContext.Uid.Equals(Owner.Uid))
                return StaticInteractionResultMeta.Default;
            
            if (meta.Point == Vector3.zero)
                meta.Point = _currentLink.Controller.PrevPos;
            
            var args = DamageArgs.GetArgs(Owner);
            AffectDamage(ref args, ref meta);
            args.Damage += AdditionalDamage;
            damageInteraction.DoDamageBullet(ref args,meta.Point, meta.Normal,_currentLink.Controller.Velocity);
            LastInteraction = LastInteractionType.Entity;

            return StaticInteractionResultMeta.InteractedBlocked;
        }
        
        protected virtual void AffectDamage(ref DamageArgs args, ref InteractionCallMeta meta)
        {
            if (DontAffectDistance)
                return;
            args.Damage *= _currentLink.LifeTime / _currentLink.InitialLifeTime;
        }
        
        public virtual InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
        {
            if (meta.Point == Vector3.zero)
                meta.Point = _currentLink.CreationData.Position;
            
            var args = DamageArgs.GetArgs(Owner);
            environment.OnHit(ref args, ref meta);
            var behaviour = environment.BehaviourData.GetBehaviour(DamageType.Range);
            if (behaviour.TryGetSound(out var clip))
            {
                if (AudioService.TryPlayQueueSound(clip, "ground" + Owner.Uid, 0.1f, out var player))
                {
                    player
                        .SetPosition(meta.Point)
                        .SetVolume(Volume)
                        .SetSpatialBlend(1);
                }
            }
            
            if (string.IsNullOrEmpty(ImpactOverrideFX))
            {
               VFXFactory.CreateAndForget(behaviour.ParticleKey, meta.Point, meta.Normal);
            }
            else
            {
                VFXFactory.CreateAndForget(ImpactOverrideFX, meta.Point, meta.Normal);
            }
            LastInteraction = LastInteractionType.Environment;

            if (behaviour.BlockInteraction)
            {
                return StaticInteractionResultMeta.InteractedBlocked;
            }
            else
            {
                return StaticInteractionResultMeta.InteractedPassed;
            }
        }
        
        public virtual InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

        public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) { return StaticInteractionResultMeta.Default; }
        public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) { return StaticInteractionResultMeta.Default; }
        public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
        #endregion
        
        
        protected enum LastInteractionType : byte
        {
            None,
            Entity,
            Environment,
        }
    }
}