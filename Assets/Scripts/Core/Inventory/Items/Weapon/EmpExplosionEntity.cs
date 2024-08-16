using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class EmpExplosionEntity : ExplosionEntity
    {
        [SerializeField] private SerilizedEffectArgs _effectArgs;
        public override InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta)
        {
            var args = _effectArgs.GetArgs(Owner);
            effectInteraction.DoEffect(ref args);
            return StaticInteractionResultMeta.Default;
        }

        public override InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
        {
            return StaticInteractionResultMeta.Default;
        }

        public override InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta)
        {
            return StaticInteractionResultMeta.Default;
        }
    }
}