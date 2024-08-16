using System;
using System.Collections.Generic;
using Core.Entity.Characters;
using Core.HealthSystem;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class EntityDamagable : AbstractMonoInteraction
    {
        [SerializeField] private MonoInteractionMiddleware _interactionMiddleware;
        [SerializeField] private bool invertNormal;
        public DamageBehaviourData BehaviourData;
        public EntityContext TargetContext;
        public List<DamageType> CooldownTypes;
        public bool DontCashDamagedUIDs;
        private IDisposable _cooldown;
        
        
        public override uint Uid => TargetContext.Uid;

        private void Start()
        {
            OnStart();
        }

        protected virtual void OnStart(){}

        public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
        {
            if (_interactionMiddleware)
            {
                var isValid = _interactionMiddleware.IsValidInteraction(this, visiter, ref meta);
                if (!isValid)
                    return StaticInteractionResultMeta.Default;
            }
            
            var res = visiter.Accept(this,ref meta);
            return res;
        }

        public virtual void DoDamageWithEffects(ref DamageArgs args, Vector3 pos, Vector3 normal, DamageType type)
        {
            if (_cooldown != null)
                return;
            
            var behaviour = BehaviourData.GetBehaviour(type);
            if (!string.IsNullOrEmpty(behaviour.ParticleKey))
            {
                if(behaviour.ParticleKey != "blood_spray" && args.BloodLossAmount > 0)
                    TargetContext.VFXFactory.CreateAndForget(behaviour.ParticleKey, pos, invertNormal ? normal : -normal);
            }
            if (behaviour.TryGetSound(out var sound))
            {
                if (TargetContext.AudioService.TryPlayQueueSound(sound, TargetContext.Uid.ToString(), 0.1f,
                        out var player))
                {
                    player
                        .SetPosition(pos)
                        .SetVolume(0.5f)
                        .SetSpatialBlend(1);
                }
            }
            DoDamage(ref args,type);
        }

        public virtual void DoDamage(ref DamageArgs args, DamageType type)
        {
            if (_cooldown != null)
                return;
            
            var behaviour = BehaviourData.GetBehaviour(type);
            behaviour.Apply(ref args);
            TargetContext.DoDamage(ref args, type);
            
            if (CooldownTypes.Contains(type))
                OnCooldown();
        }

        public virtual void DoDamageFire(ref DamageArgs args)
        {
            DoDamageWithEffects(ref args, transform.position, transform.position, DamageType.Range);
        }

        public virtual void DoDamageBullet(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 bulletVel)
        {
            DoDamageWithEffects(ref args, pos, normal, DamageType.Range);
        }
        
        public virtual void DoDamageMelee(ref DamageArgs args, Vector3 pos, Vector3 normal)
        {
            DoDamageWithEffects(ref args, pos, normal, args.DamageType);
        }
        
        public virtual void DoDamageExplosion(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 direction)
        {
            DoDamageWithEffects(ref args, pos, normal, DamageType.Explosion);
        }

        private void OnCooldown()
        {
            _cooldown?.Dispose();
            _cooldown = Observable.Timer(5f.ToSec())
                .Finally(() =>
                {
                    _cooldown = null;
                })
                .Subscribe(_ =>
                {
                    _cooldown?.Dispose();
                });
        }

        private void OnDisable()
        {
            _cooldown?.Dispose();
        }

#if UNITY_EDITOR
        [Button]
        public void EditorDoDamage()
        {
            var dmgArgs = new DamageArgs(null, 10f);
            DoDamageBullet(ref dmgArgs, transform.position, transform.forward, Vector3.zero);
        }
#endif
    }

}