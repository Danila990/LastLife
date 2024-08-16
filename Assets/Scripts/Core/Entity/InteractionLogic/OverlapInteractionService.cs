using System.Buffers;
using BurstLinq;
using Core.Entity.Repository;
using SharedUtils;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace Core.Entity.InteractionLogic
{
    public interface IOverlapInteractionService
    {
        void OverlapSphere(IInteractorVisiter visiter, Vector3 point, float radius, uint selfUid = 0, int layerMask = 0);
        void RepositorySphereCast(IInteractorVisiter visiter, Vector3 point, float radius, uint selfUid = 0);
    }
    
    public class OverlapInteractionService : IOverlapInteractionService
    {
        private readonly IEntityRepository _repository;

        public OverlapInteractionService(
            IEntityRepository repository
            )
        {
            _repository = repository;
        }
        
        public void OverlapSphere(IInteractorVisiter visiter, Vector3 point, float radius, uint selfUid = 0, int layerMask = 0)
        {
            if (layerMask == 0)
                layerMask = LayerMasks.RagdollObject;
            
            var cashedIds = ListPool<uint>.Get();
            var pool = ArrayPool<Collider>.Shared.Rent(50);
            var count = Physics.OverlapSphereNonAlloc(point, radius, pool, layerMask);
            Util.DrawSphere(point, Quaternion.identity, radius, Color.black);
            cashedIds.Add(selfUid);
            
            for (var i = 0; i < count; i++)
            {
                var collider = pool[i];
                if (!collider.TryGetComponent(out IInteractableProvider provider)) 
                    continue;
                if (provider.Uid != 0 && BurstLinqExtensions.Contains(cashedIds, provider.Uid))
                    continue;

                var collisionPoint = collider is MeshCollider { convex:false } ? point : collider.ClosestPoint(point);
                var meta = new InteractionCallMeta
                {
                    Point = collisionPoint,
                    OriginPoint = point,
                };
                Debug.DrawLine(meta.Point, meta.OriginPoint, Color.magenta, 5, false);

                var res = provider.Visit(visiter, ref meta);
                if(res.DontCache) 
                    continue;
                cashedIds.Add(provider.Uid);
            }
            
            ListPool<uint>.Release(cashedIds);
            ArrayPool<Collider>.Shared.Return(pool);
        }
        
        public void RepositorySphereCast(IInteractorVisiter visiter, Vector3 point, float radius, uint selfUid = 0)
        {
            var cashedIds = ListPool<uint>.Get();
            cashedIds.Add(selfUid);
            var sqrRadius = radius * radius;
            foreach (var entity in _repository.EntityContext)
            {
                var delta = entity.MainTransform.position - point;
                var dist = delta.sqrMagnitude;
                if (dist > sqrRadius) continue;
                if (entity.Uid != 0 && BurstLinqExtensions.Contains(cashedIds, entity.Uid)) continue;
                cashedIds.Add(entity.Uid);
                var meta = new InteractionCallMeta
                {
                    Point = point,
                    OriginPoint = point
                };
                entity.Visit(visiter,ref meta);
            }
            ListPool<uint>.Release(cashedIds);
        }
    }
}