using Core.Entity.Characters.Adapters;
using UnityEngine;
using Utils;

namespace Core.Factory.DataObjects
{
    [CreateAssetMenu(menuName = SoNames.FACTORY_DATA+nameof(MechFactoryData),fileName = nameof(MechFactoryData))]
    public class MechFactoryData : ScriptableObject,IMechFactoryData
    {
        [SerializeField] private MechCharacterAdapter _adapter;
        public MechCharacterAdapter Adapter => _adapter;
    }
}