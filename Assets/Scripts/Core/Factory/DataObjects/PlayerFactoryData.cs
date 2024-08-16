using Core.Entity.Characters.Adapters;
using UnityEngine;
using Utils;

namespace Core.Factory.DataObjects
{
    [CreateAssetMenu(menuName = SoNames.FACTORY_DATA+nameof(PlayerFactoryData),fileName = nameof(PlayerFactoryData))]
    public class PlayerFactoryData : ScriptableObject,IPlayerFactoryData
    {
        [SerializeField] private PlayerCharacterAdapter _adapter;
        public PlayerCharacterAdapter Adapter => _adapter;
    }
}