using System.Collections.Generic;
using Core.Entity;
using Cysharp.Threading.Tasks;
using Db.VFXDataDto.Impl;
using GameSettings;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Projectile
{
    public class SimpleProjectile : ProjectileEntity
    {
        [Optional, SerializeField] private VFXContext _particle;
        protected VFXContext Particle => _particle;
        
        public override void OnHitTarget(IReadOnlyList<RaycastHit> targets, int hitCount, EntityContext source, out bool isBlocking, out RaycastHit hit)
        {
            isBlocking = false;
            hit = new RaycastHit();
            for (var index = 0; index < hitCount; index++)
            {
                var target = targets[index];
                HandleHit(target, out var blocking);
                isBlocking = blocking;
                hit = target;
                
                if (blocking) 
                    return;
            }
        }

        public override void Release()
        {
            if (ReleaseDelay > 0)
            {
                DelayRelease().Forget();
            }
            else
            {
                OnReturnToPool();
                Pool.Return(this);
            }
        }

        public override void OnRent()
        {
            base.OnRent();
            if (GameSetting.ViolenceStatus)
            {
                gameObject.SetActive(false);
            }
            if (_particle)
            {
                _particle.gameObject.SetActive(true);
                _particle.Play();
            }
        }

        protected async UniTaskVoid DelayRelease()
        {
            await UniTask.Delay(ReleaseDelay.ToSec(), cancellationToken: destroyCancellationToken);
            if (_particle)
            {
                _particle.Stop();
                _particle.gameObject.SetActive(false);
            }
            OnReturnToPool();
            Pool.Return(this);
        }

        protected virtual void OnReturnToPool() { }
    }
}