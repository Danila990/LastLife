using System.Collections.Generic;
using System.Linq;
using Core.Entity;
using Core.Entity.Repository;
using Core.Factory.DataObjects;
using Core.Inventory.Items;
using Core.Projectile.Projectile;
using UnityEngine;
using VContainer.Unity;

namespace Core.Factory
{
    public class ThrowableFactory : IThrowableFactory, IInitializable
    {
        private readonly IFactoryData _factoryData;
        private readonly IObjectFactory _factory;
        private readonly Dictionary<string, EntityContext> _objects = new();

        public ThrowableFactory(
            IFactoryData factoryData,
            IObjectFactory factory
        )
        {
            _factoryData = factoryData;
            _factory = factory;
        }

        public ThrowableEntity CreateObject(string key, Vector3 pos, Quaternion rot)
        {
            if (!_objects.ContainsKey(key))
            {
                Debug.LogError($"Factory dosen't contais {key} object!");
                return null;
            }

            var instance = _factory.CreateObject(key,pos,rot);
            return instance as ThrowableEntity;
        }

        public ThrowableEntity CreateObject(string key, Vector3 pos)
        {
            return CreateObject(key, pos, Quaternion.identity);
        }

        public ThrowableEntity CreateObject(string key)
        {
            return CreateObject(key, Vector3.zero, Quaternion.identity);
        }
        
        public ThrowableEntity CreateObject(ref ThrowableCreationData creationData)
        {
            var throwableEntity = CreateObject(creationData.ThrowableKey, creationData.Position, Quaternion.LookRotation(creationData.Direction.normalized));
            throwableEntity.Owner = creationData.Source;
            throwableEntity.TargetRigidbody.velocity = creationData.Direction * creationData.Velocity + creationData.ExtraVelocity;
            return throwableEntity;
        }

        public void Initialize()
        {
            var throwables = _factoryData.Objects.Where(x => x.Type == EntityType.Throwable);
            foreach (var obj in throwables)
            {
#if UNITY_EDITOR
                if (_objects.ContainsKey(obj.Key))
                {
                    Debug.LogError($"Factory already contains {obj.Key}!");
                    continue;
                }
#endif
                _objects.Add(obj.Key,obj.Object);
            }
        }
    }

}