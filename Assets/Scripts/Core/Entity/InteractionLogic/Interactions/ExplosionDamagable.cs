using Core.Entity.Characters;
using Core.HealthSystem;
using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class ExplosionDamagable : EntityDamagable
    {
        [SerializeField] private RemoteExplosionEntity _remoteExplosion;
        public override void DoDamage(ref DamageArgs args, DamageType type)
        {
            _remoteExplosion.RemoteDetonate();
        }

        public override void DoDamageWithEffects(ref DamageArgs args, Vector3 pos, Vector3 normal, DamageType type)
        {
            _remoteExplosion.RemoteDetonate();
        }
    }
}