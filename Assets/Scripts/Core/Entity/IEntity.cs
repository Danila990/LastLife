using UnityEngine;

namespace Core.Entity
{
    public interface IEntity : IDamageSource
    {
        uint Uid { get; }
        Transform MainTransform { get; }
        Transform LookAtTransform { get; }
        bool IsDestroyed { get; }
    }

    public interface IDamageSource
    {
        public string SourceId {get;}
    }

    public class MetaDamageSource : IDamageSource
    {
        public string SourceId { get; }

        public MetaDamageSource(string sourceId)
        {
            SourceId = sourceId;
        }
    }
    
    public interface IOwnedEntity
    {
        EntityContext Owner { get; set; }
    }
}