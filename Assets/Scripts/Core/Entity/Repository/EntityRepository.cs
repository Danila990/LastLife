using System;
using System.Collections;
using System.Collections.Generic;
using Core.HealthSystem;
using MessagePipe;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Repository
{
    public class EntityRepository : IEntityRepository
    {
        private readonly IPublisher<RepositoryEvent> _repositoryEvent;

        [ShowInInspector]
        private readonly HashSet<EntityContext> _entityContexts = new HashSet<EntityContext>();
        
        public IReadOnlyCollection<EntityContext> EntityContext => _entityContexts;

        [ShowInInspector]
        private readonly Dictionary<Type, IEnumerable> _contexts = new Dictionary<Type, IEnumerable>();
        
        private readonly EntityDeathPublisher _onDeathSubscriber;
        private readonly Dictionary<uint, EntityContext> _entitiesById = new Dictionary<uint, EntityContext>();
        private readonly Type _entityType;

        public EntityRepository(IPublisher<RepositoryEvent> repositoryEvent, IPublisher<MessageEntityDeath> messageEntityDeath)
        {
            _repositoryEvent = repositoryEvent;
            _onDeathSubscriber = new EntityDeathPublisher(messageEntityDeath);
            _entityType = typeof(EntityContext);
        }

        public bool GetEntityById(uint id, out EntityContext entityContext)
            => _entitiesById.TryGetValue(id, out entityContext);
        
        public void AddGenericEntity<T>(T entityContext) where T : EntityContext
        {
            if (entityContext is LifeEntity lifeEntity)
            {
                _onDeathSubscriber.ObserveEntity(lifeEntity);
            }
            
            if (_contexts.TryGetValue(typeof(T), out var enumerable))
            {
                var set = (HashSet<T>)enumerable;
                set.Add(entityContext);
                // The event is not called the first time it is added
                _repositoryEvent.Publish(new RepositoryEvent(typeof(T), entityContext, RepositoryEvent.EventType.Add, set.Count, true));
            }
            else
            {
                _contexts[typeof(T)] = new HashSet<T>
                { entityContext };
            }
        }

        public ICollection<T> GetGenericEntities<T>() where T : EntityContext
        {
            if (_contexts.TryGetValue(typeof(T), out var hashSet))
            {
                return hashSet as ICollection<T>;
            }
            else
            {
                return (ICollection<T>)(_contexts[typeof(T)] = new HashSet<T>());
            }
        }
        
        public void RemoveGenericEntity<T>(T entityContext) where T : EntityContext
        {
            if (_contexts.TryGetValue(typeof(T), out var enumerable))
            {
                var set = (HashSet<T>)enumerable;
                set.Remove(entityContext);
                _repositoryEvent.Publish(new RepositoryEvent(typeof(T), entityContext, RepositoryEvent.EventType.Remove, set.Count, true));
            }
            else
            {
                Debug.LogError($"None Hashset as {typeof(T).Name}");
            }
        }
        
        public void AddEntity(EntityContext entityContext)
        {
            _entityContexts.Add(entityContext);
            _entitiesById.Add(entityContext.Uid, entityContext);
            _repositoryEvent.Publish(new RepositoryEvent(_entityType, entityContext, RepositoryEvent.EventType.Add, _entityContexts.Count, false));

        }

        public void RemoveEntity(EntityContext context)
        {
            _entityContexts.Remove(context);
            _entitiesById.Remove(context.Uid);
            _repositoryEvent.Publish(new RepositoryEvent(_entityType, context, RepositoryEvent.EventType.Remove, _entityContexts.Count, false));

        }

        private class EntityDeathPublisher : IObserver<DiedArgs>
        {
            private readonly IPublisher<MessageEntityDeath> _entityDeath;
            private readonly Dictionary<uint, IDisposable> _disposables = new Dictionary<uint, IDisposable>();
            
            public EntityDeathPublisher(IPublisher<MessageEntityDeath> entityDeath)
            {
                _entityDeath = entityDeath;
            }
            
            public void OnCompleted()
            {
            }
            
            public void OnError(Exception error)
            {
            }
            
            public void OnNext(DiedArgs value)
            {
                if (value.SelfEntity is null)
                    return;
                
                _disposables[value.SelfEntity.Uid]?.Dispose();
                _entityDeath.Publish(new MessageEntityDeath(value));
            }
            
            public void ObserveEntity(LifeEntity lifeEntity)
            {
                if (lifeEntity.Health.OnDeath is null)
                {
                    Debug.LogError("Health not initialized for " + lifeEntity.name);
                    return;
                }
                _disposables[lifeEntity.Uid] = lifeEntity.Health.OnDeath.Subscribe(this);
            }
        }
    }
    

    public readonly struct RepositoryEvent
    {
        public readonly Type EntityType;
        public readonly EntityContext EntityContext;
        public readonly EventType EvtType;
        public readonly int CurrentCount;
        public readonly bool GenericEntity;
        
        public RepositoryEvent(
            Type entityType,
            EntityContext entityContext,
            EventType evtType, 
            int currentCount,
            bool genericEntity)
        {
            EntityType = entityType;
            EntityContext = entityContext;
            EvtType = evtType;
            CurrentCount = currentCount;
            GenericEntity = genericEntity;
        }

        public enum EventType
        {
            Add,
            Remove
        }
    }
}