using System.Collections.Generic;
using System.Linq;
using Core.Entity;
using Core.Entity.Repository;
using Core.Factory.DataObjects;
using Core.Projectile;
using Core.Projectile.Projectile;
using UnityEngine;
using VContainer.Unity;

namespace Core.Factory
{
    public class ProjectileFactory : IProjectileFactory, IInitializable
    {
        private readonly IFactoryData _factoryData;
        private readonly IEntityRepository _repository;
        private readonly IObjectFactory _factory;
        private readonly Dictionary<string, EntityContext> _objects = new();
        private readonly Dictionary<string, float> _masses = new();
        private readonly Dictionary<string, EntityPool<ProjectileEntity>> _pools = new();

        public ProjectileFactory(
            IFactoryData factoryData,
            IEntityRepository repository,
            IObjectFactory factory
        )
        {
            _factoryData = factoryData;
            _repository = repository;
            _factory = factory;
        }

        public ProjectileEntity CreateObject(string key, Vector3 pos, Quaternion rot)
        {
            if (!_objects.ContainsKey(key))
            {
                Debug.LogError($"Factory dosen't contais {key} object!");
                return null;
            }

            var instance = _pools[key].Rent();
            instance.SetPool(_pools[key]);
            instance.transform.SetPositionAndRotation(pos, rot);
            instance.OnRent();

            return instance;
        }

        public ProjectileEntity CreateObject(string key, Vector3 pos)
        {
            return CreateObject(key, pos, Quaternion.identity);
        }

        public ProjectileEntity CreateObject(string key)
        {
            return CreateObject(key, Vector3.zero, Quaternion.identity);
        }
        
        public ProjectileEntity CreateObject(ref ProjectileCreationData creationData)
        {
            var projectileEntity = CreateObject(creationData.ProjectileKey, creationData.Position, Quaternion.LookRotation(creationData.Direction.normalized));
            projectileEntity.Owner = creationData.Source;
            
            return projectileEntity;
        }

        public float GetProjectileMass(string key)
        {
            return _masses[key];
        }

        public void Initialize()
        {
            var projectiles = _factoryData.Objects.Where(x => x.Type == EntityType.Projectile);
            foreach (var obj in projectiles)
            {
                if (_objects.ContainsKey(obj.Key))
                {
                    Debug.LogError($"Factory already contains {obj.Key}!");
                    continue;
                }
                _objects.Add(obj.Key, obj.Object);
                var pool = new EntityPool<ProjectileEntity>(_factory, _repository, obj.Key);
                pool.Prewarm(5);
                _pools.Add(obj.Key, pool);
                if (obj.Object is ProjectileEntity bullet)
                {
                    _masses.Add(obj.Key, bullet.BulletWeight);
                }
            }
        }
    }
}