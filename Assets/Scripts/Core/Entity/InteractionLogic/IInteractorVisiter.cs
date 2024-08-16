using Core.Entity.InteractionLogic.Interactions;

namespace Core.Entity.InteractionLogic
{
    public interface IInteractorVisiter
    {
        InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(EntityEffectable damagable, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        InteractionResultMeta Accept(TriggerInteraction interaction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
    }
}