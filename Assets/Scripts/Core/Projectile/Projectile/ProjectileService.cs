using System;
using System.Buffers;
using System.Collections.Generic;
using Core.Factory;
using UnityEngine;
using Utils;
using VContainer.Unity;

namespace Core.Projectile.Projectile
{
    public class ProjectileService : IFixedTickable, IDisposable, IProjectileService, ITickable
    {
        private readonly IProjectileFactory _projectileFactory;
        private readonly HashSet<ProjectileLink> _projectileLinks = new HashSet<ProjectileLink>();
        private readonly Stack<ProjectileLink> _toDestroy = new Stack<ProjectileLink>();
        private RaycastSort _raycastSort = new RaycastSort();
        //private readonly Vector3 _castSize = new Vector3(0.07f, 0.01f, 0.01f);

        public ProjectileService(IProjectileFactory projectileFactory)
        {
            _projectileFactory = projectileFactory;
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime; 
            foreach (var projectileLink in _projectileLinks)
            {
                if (projectileLink.FrameSkip)
                {
                    projectileLink.FrameSkip = false;
                    continue;
                }
                projectileLink.View.ProjectileUpdate();
                projectileLink.LifeTime -= deltaTime;
                if (projectileLink.LifeTime <= 0)
                {
                    projectileLink.LifeTime = projectileLink.InitialLifeTime;
                    ReleaseProjectile(projectileLink);
                    continue;
                }
                var newPosition =
                    CalculateNewPosition(projectileLink.View, ref projectileLink.Controller, ref deltaTime);
                projectileLink.View.transform.SetPositionAndRotation(newPosition,
                    Quaternion.LookRotation(projectileLink.Controller.Velocity.normalized));
                projectileLink.Controller.Position = newPosition;
                projectileLink.PosUpdated = true;
            }
        }
        
        public void FixedTick()
        {
            var pool = ArrayPool<RaycastHit>.Shared.Rent(5);
            foreach (var projectileLink in _projectileLinks)
            {
                if (!projectileLink.PosUpdated)
                {
                    continue;
                }
                var hitCount = RayCastProjectile(
                    ref projectileLink.Controller,
                    in LayerMasks.HitObjectMask, 
                    projectileLink, pool);

                if (hitCount > 0)
                {
                    projectileLink.View.OnHitTarget(pool, hitCount, projectileLink.Source, out var isBlocking,out var hit);
                    if (isBlocking)
                    {
                        projectileLink.View.transform.position = hit.point;
                        projectileLink.View.OnCollision(this);
                    }
                }
            }
            
            ArrayPool<RaycastHit>.Shared.Return(pool);
            while (_toDestroy.TryPop(out var result))
            {
                result.View.Release();
                _projectileLinks.Remove(result);
            }
        }

        public void ReleaseProjectile(ProjectileLink projectileLink)
        {
            if (projectileLink.Destroyed)
                return;
            
            projectileLink.Destroyed = true;
            _toDestroy.Push(projectileLink);
        }

        private int RayCastProjectile(ref ProjectileController controller,
            in int layerMask,
            ProjectileLink projectileLink,
            RaycastHit[] rayCastHits)
        {
            var size = projectileLink.View.BulletSphereSize > 0 ? projectileLink.View.BulletSphereSize : 0.05f;
            var direction = controller.Position - controller.PrevPos;
            var count = Physics.SphereCastNonAlloc(
                controller.PrevPos, 
                size, 
                direction.normalized, 
                rayCastHits,
                direction.magnitude * 2f, 
                layerMask);
            
            Array.Sort(rayCastHits,0,count, _raycastSort);
            controller.PrevPos = controller.Position;
            return count;
        }

        private Vector3 CalculateNewPosition(ProjectileEntity view, ref ProjectileController controller,
            ref float deltaTime)
        {
            var gravity = (Physics.gravity * deltaTime * deltaTime * controller.Mass) / 2;
            controller.Velocity += gravity;
            return controller.Position + controller.Velocity * deltaTime;
        }


        public void CreateProjectile(ProjectileCreationData creationData)
        {
            CreateProjectile(creationData, out var link);
        }

        public void CreateProjectile(ProjectileCreationData creationData, out ProjectileLink projectileLink)
        {
            var projectile = _projectileFactory.CreateObject(ref creationData);//TODO: SELECT PROJECTILE
            var controller = new ProjectileController
            {
                Position = creationData.Position,
                Velocity = creationData.Direction * creationData.Velocity + creationData.ExtraVelocity,
                PrevPos =  creationData.Position,
                Mass = projectile.BulletWeight
            };
            
            var newLink = new ProjectileLink(creationData, projectile, controller, creationData.Source)
            {
                LifeTime = creationData.LifeTime,
                InitialLifeTime = creationData.LifeTime
            };
            projectile.SetLink(newLink);

            _projectileLinks.Add(newLink);
            projectileLink = newLink;
        }

        public void Dispose()
        {
        }
    }
}