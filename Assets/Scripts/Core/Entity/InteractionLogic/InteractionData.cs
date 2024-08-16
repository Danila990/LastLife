using System;
using System.Collections.Generic;
using Core.Entity.InteractionLogic.Interactions;
using Sirenix.OdinInspector;

namespace Core.Entity.InteractionLogic
{
    [Serializable]
    public class InteractionData
    {
        public InteractionType InteractionType;

        [ShowIf("InteractionType", InteractionType.ScriptableInteraction)]
        public AbstractSoInteraction SoInteraction;

        [ShowIf("InteractionType", InteractionType.MonoInteraction)]
        public AbstractMonoInteraction MonoInteraction;

        [ShowIf("InteractionType", InteractionType.GlobalInteraction)]
        [ValueDropdown("GetKeys")]
        public string InteractionKey;
        [ShowIf("InteractionType", InteractionType.GlobalInteraction)] public GlobalInteractionData ReferenceData;
        [NonSerialized] public IInteractableContexted InterfaceInteraction;

        public bool HasPriority;
        [ShowIf("HasPriority")]
        public int Priority;
        
        private IEnumerable<string> GetKeys()
        {
            return !ReferenceData ? new List<string>() : ReferenceData.GetKeys();
        }

        public IInteractableContexted GetInteraction()
        {
            return InteractionType switch
            {
                InteractionType.ScriptableInteraction => SoInteraction,
                InteractionType.MonoInteraction => MonoInteraction,
                InteractionType.GlobalInteraction => ReferenceData.GetDataByKey(InteractionKey),
                InteractionType.InterfaceInteraction => InterfaceInteraction,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}