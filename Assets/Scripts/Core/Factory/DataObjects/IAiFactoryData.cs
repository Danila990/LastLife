using Core.Entity.Characters.Adapters;

namespace Core.Factory.DataObjects
{
    public interface IAiFactoryData
    {
        public AiBindedCharacter[] Characters { get; }
        public AiBindedHead[] Heads { get; }
    }
    
    public interface IPlayerFactoryData
    {
        public PlayerCharacterAdapter Adapter { get; }
    }
    
    public interface IMechFactoryData
    {
        public MechCharacterAdapter Adapter { get; }
    }
}