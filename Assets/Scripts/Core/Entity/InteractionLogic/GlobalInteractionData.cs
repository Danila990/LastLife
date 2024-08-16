using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Core.Entity.InteractionLogic
{
    [CreateAssetMenu(menuName = SoNames.INTERACTION_DATA + nameof(GlobalInteractionData),fileName = nameof(GlobalInteractionData))]
    public class GlobalInteractionData : ScriptableObject
    {
        public InteractionData[] InteractionsData;
        
        public IEnumerable<string> GetKeys()
        {
            return InteractionsData.Select(x => x.GetInteraction().GetType().Name);
        }

        public IInteractableContexted GetDataByKey(string key)
        {
            var data = InteractionsData.FirstOrDefault(x => x.GetInteraction().GetType().Name == key);
            if (data is null)
            {
                throw new NullReferenceException($"GLOBAL Interaction data [{key}] not found!");
            }
            return data.GetInteraction();
        }
    }
}