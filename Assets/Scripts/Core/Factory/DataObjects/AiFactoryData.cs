using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Factory.DataObjects
{
    [CreateAssetMenu(menuName = SoNames.FACTORY_DATA + nameof(AiFactoryData), fileName = nameof(AiFactoryData))]
    public class AiFactoryData : ScriptableObject, IAiFactoryData
    {
        [SerializeField] private AiBindedCharacter[] _characters;
        [SerializeField] private AiBindedHead[] _heads;
        public AiBindedCharacter[] Characters => _characters;
        public AiBindedHead[] Heads => _heads;

#if UNITY_EDITOR
        [SerializeField] private FactoryData _referenceData;
        [OnInspectorInit]
        public void RecalcIds()
        {
            if(!_referenceData) return;
            var ids = _referenceData.Objects.Where(obj => obj.Type.Equals(EntityType.Character))
                .Select(obj => obj.Key);

            var availableIds = ids as string[] ?? ids.ToArray();
            foreach (var character in _characters)
            {
                var aiBindedCharacter = character;
                aiBindedCharacter.AvailableIds = availableIds;
            }  
            foreach (var character in _heads)
            {
                var aiBindedCharacter = character;
                aiBindedCharacter.AvailableIds = availableIds;
            }  
        }
#endif
    }
}