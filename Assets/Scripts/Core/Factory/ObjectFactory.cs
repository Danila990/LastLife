using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Repository;
using Core.Factory.DataObjects;
using Core.Services.SaveSystem.SaveAdapters.EntitySave;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Core.Factory
{
    public class ObjectFactory : IObjectFactory, IInitializable
    {
        private readonly IFactoryData _factoryData;
        private readonly IObjectResolver _resolver;
        private readonly IEntityRepository _repository;
        private readonly Dictionary<string, EntityContext> _objects = new();
        private Transform _holder;
        private static uint _throughUid = 1;
        public static uint GetNextUid() => ++_throughUid;
        public Transform Holder => _holder;

        public ObjectFactory(
            IFactoryData factoryData,
            IObjectResolver resolver,
            IEntityRepository repository
        )
        {
            _factoryData = factoryData;
            _resolver = resolver;
            _repository = repository;
        }

        public EntityContext CreateObject(string key, Vector3 pos, Quaternion rot, bool addToRepository = true)
        {
#if UNITY_EDITOR
            if (!_objects.TryGetValue(key, out var o))
            {
                Debug.LogError($"Factory doesn't contains {key} object!");
                return null;
            }
            var instance = Object.Instantiate(o, pos, rot, _holder);
#else
            var instance = Object.Instantiate(_objects[key], pos, rot, _holder);
#endif       
            InitializeEntity(key, addToRepository, instance);

            return instance;
        }
        
        private void InitializeEntity(string key, bool addToRepository, EntityContext instance)
        {

            if (instance is ComplexEntity complexEntity)
            {
                if (complexEntity.MainEntity)
                {
                    instance = complexEntity.MainEntity;
                }
                
                complexEntity.ManualInit(_resolver, key);
            }
            else
            {
                _resolver.Inject(instance);
                instance.Created(_resolver, key);
            }

            instance.Uid = GetNextUid();
            instance.name += instance.Uid.ToString();
            _repository.AddEntity(instance);

            if (addToRepository && instance is LifeEntity lifeEntity)
            {
                _repository.AddGenericEntity(lifeEntity);
            }

            if (instance is ISavableEntity savableEntity)
            {
                savableEntity.FactoryId = key;
            }
        }

        public EntityContext CreateObject(string key, Vector3 pos, bool addToRepository = true)
        {
           return CreateObject(key, pos, Quaternion.identity, addToRepository);
        }

        public EntityContext CreateObject(string key)
        {
           return CreateObject(key, Vector3.zero, Quaternion.identity);
        }

        public EntityContext CreateFromPrefab(EntityContext prefab, string key)
        {
            var instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, _holder);
            
            InitializeEntity(key, true, instance);

            return instance;
        }


        public void Initialize()
        {
            _holder = new GameObject("--- Object Holder ---").transform;
            foreach (var obj in _factoryData.Objects)
            {
                if (_objects.ContainsKey(obj.Key))
                {
                    Debug.LogError($"Factory already contains {obj.Key}!");
                    continue;
                }
                _objects.Add(obj.Key, obj.Object);
            }
        }
    }
}