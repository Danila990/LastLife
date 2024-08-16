using Core.Entity.Characters;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public abstract class AbstractSoInteraction : ScriptableObject, IInteractableContexted
    {
        public virtual void SetCharContext(CharacterContext context) {}
        public virtual void SetEntityContext(EntityContext context) {}

        public uint Uid { get; set; }

        public abstract InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta);
    }
}