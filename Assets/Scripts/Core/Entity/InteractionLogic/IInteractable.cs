using Core.Entity.Characters;
using UnityEngine;

namespace Core.Entity.InteractionLogic
{
    public interface IInteractable
    {
        uint Uid { get; }
        InteractionResultMeta Visit(IInteractorVisiter visiter,ref InteractionCallMeta meta);
    }

    public interface IInteractableProvider : IInteractable
    {
    }

    public interface IInteractableContexted : IInteractable
    {
        void SetCharContext(CharacterContext context);
        void SetEntityContext(EntityContext context);
    }

    public struct InteractionResultMeta
    {
        public bool Interacted;
        public bool HitBlock;
        public bool DontCache;

        public InteractionResultMeta(bool interacted, bool hitBlock)
        {
            Interacted = interacted;
            HitBlock = hitBlock;
            DontCache = false;
        }
    }

    public static class StaticInteractionResultMeta
    {
        private static InteractionResultMeta _default = new InteractionResultMeta(false,false);
        private static InteractionResultMeta _interactedBlocked = new InteractionResultMeta(true,true);
        private static InteractionResultMeta _interactedPassed = new InteractionResultMeta(true,false);
        private static InteractionResultMeta _blocked = new InteractionResultMeta(false,true);
        
        public static ref InteractionResultMeta Default => ref _default;
        public static ref InteractionResultMeta InteractedBlocked => ref _interactedBlocked;
        public static ref InteractionResultMeta InteractedPassed => ref _interactedPassed;
        public static ref InteractionResultMeta Blocked => ref _blocked;
    }

    public struct InteractionCallMeta
    {
        public Vector3 Point;
        public Vector3 OriginPoint;
        public Vector3 Normal;
        public Collider Collider;
        public float Distance;
    }
}