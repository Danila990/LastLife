using Core.Entity.Characters;
using Core.HealthSystem;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class EnviromentProjectileInteraction : AbstractMonoInteraction, IInteractableProvider
    {
        public DamageBehaviourData BehaviourData;
        public override uint Uid => 9999;
        public virtual bool AcceptMelee => false;
        
        public override InteractionResultMeta Visit(IInteractorVisiter visiter,ref InteractionCallMeta meta)
        {
            return visiter.Accept(this, ref meta);
        }

        public virtual void OnHit(ref DamageArgs args, ref InteractionCallMeta meta)
        {
            
        }
    }

}