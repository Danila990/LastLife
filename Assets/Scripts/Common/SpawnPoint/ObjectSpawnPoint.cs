using System.Collections.Generic;
using System.Linq;
using Core.Entity;
using Core.Factory;
using Core.Factory.DataObjects;
using Db.ObjectData;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Common.SpawnPoint
{
    [SelectionBase]
    public class ObjectSpawnPoint : SpawnPoint
    {
        [SerializeField] protected ItemObject _objectData;
        
        public override void Create(IAdapterStrategyFactory strategyFactory)
        {
            strategyFactory.CreateObject(_objectData, transform.position, transform.rotation);
            Destroy(gameObject,Random.Range(0.01f, 1f));
        }
        
        public virtual EntityContext CreateObject(IObjectFactory objectFactory, bool destroy)
        {
            if (destroy)
            {
                Destroy(gameObject,Random.Range(0.1f, 1f));
            }
            var entity = objectFactory.CreateObject(_objectData.FactoryId, transform.position, transform.rotation);
           
            return entity;
        }

#if UNITY_EDITOR
        private EntityContext _cashedEntity;
        private IEnumerable<MeshFilter> _meshFilters;
        private IEnumerable<SkinnedMeshRenderer>  _skinnedMeshRenderers;
        
        [Button]
        private void Reinit()
        {
            _cashedEntity = FactoryData.EditorInstance.Objects.FirstOrDefault(data => data.Key == _objectData.FactoryId).Object;
            if (_cashedEntity == null)
                return;
            
            _meshFilters = _cashedEntity.GetMeshFilters();
            _skinnedMeshRenderers = _cashedEntity.GetSkinnedMeshRenderers();
        }
        
        private void OnDrawGizmosSelected()
        {
            _cashedEntity ??= FactoryData.EditorInstance.Objects.FirstOrDefault(data => data.Key == _objectData.FactoryId).Object;
            if (_cashedEntity == null)
                return;
            
            _meshFilters ??= _cashedEntity.GetMeshFilters();
            if (_meshFilters != null)
            {
                foreach (var rend in _meshFilters)
                {
                    if (rend.sharedMesh.triangles.Length <= 0)
                        continue;   
                    Gizmos.DrawWireMesh(rend.sharedMesh,
                        transform.position + rend.transform.localPosition,
                        transform.rotation * rend.transform.rotation,
                        rend.transform.lossyScale);
                }
            }
            
            _skinnedMeshRenderers ??= _cashedEntity.GetSkinnedMeshRenderers();
            if (_skinnedMeshRenderers != null)
            {
                foreach (var rend in _skinnedMeshRenderers)
                {
                    Gizmos.DrawWireMesh(
                        rend.sharedMesh,
                        transform.position + rend.transform.localPosition, 
                        transform.rotation * rend.transform.rotation, 
                        rend.transform.localScale);
                }
            }
        }
#endif
    }

}