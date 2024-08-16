using Core.Entity;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using SharedUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class FlamethrowerOrigin : EntityContext
    {
        public EntityContext CurrentContext;
        public ParticleSystem Part;

        private float DamageDelay = 0.3f;
        private SerilizedEffectArgs EffectArgs;
        private SerializedDamageArgs DamageArgs;
        private float _timeEnd;

        public void Init(SerilizedEffectArgs serilizedEffectArgs, SerializedDamageArgs serializedDamageArgs, float damageDelay)
        {
            EffectArgs = serilizedEffectArgs;
            DamageArgs = serializedDamageArgs;
            DamageDelay = damageDelay;
            _timeEnd = Time.time;
            Deactivate();
        }

        public void Activate()
        {
            Part.Play();
        }

        public void Deactivate()
        {
            Part.Stop();
        }

        private void OnParticleCollision(GameObject other)
        {
            if (_timeEnd > Time.time)
                return;

            if (other.TryGetComponent(out EntityDamagable damageComponet))
            {
                if (CurrentContext == damageComponet.TargetContext)
                    return;
                _timeEnd = Time.time + DamageDelay;
                var damageArgs = DamageArgs.GetArgs(this);
                damageComponet.DoDamageFire(ref damageArgs);
                if (other.TryGetComponent(out EntityEffectable effectComponent))
                {
                    var args = EffectArgs.GetArgs(this);
                    effectComponent.DoEffect(ref args);
                }
            }
        }
    }
}