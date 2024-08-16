using Core.Entity.Characters;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public abstract class AbstractMonoInteraction : MonoBehaviour, IInteractableContexted
    {
        public virtual void SetCharContext(CharacterContext context) {}
        public virtual void SetEntityContext(EntityContext context) {}
        
        public virtual uint Uid => 0;
        
        public virtual InteractionResultMeta Visit(IInteractorVisiter visiter,ref InteractionCallMeta meta)
        {
            return new InteractionResultMeta();
        }
    }
}