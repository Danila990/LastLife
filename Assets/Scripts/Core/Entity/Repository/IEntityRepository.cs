using System.Collections.Generic;

namespace Core.Entity.Repository
{
    public interface IEntityRepository
    {
        void AddEntity(EntityContext entityContext);
        void RemoveEntity(EntityContext context);
        
        bool GetEntityById(uint id, out EntityContext context);
        void AddGenericEntity<T>(T entityContext) where T : EntityContext;
        ICollection<T> GetGenericEntities<T>()  where T : EntityContext; 
        void RemoveGenericEntity<T>(T entityContext) where T : EntityContext;
        
        IReadOnlyCollection<EntityContext> EntityContext { get; }
    }
}