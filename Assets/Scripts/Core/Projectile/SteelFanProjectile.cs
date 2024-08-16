using System.Collections.Generic;
using System.Threading;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Projectile.Projectile;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Projectile
{
    public class SteelFanProjectile : SimpleProjectile
    {
        public TrailRenderer Trail;
        public Transform KnifeModel;
        public float RotationTime;
        public LoopType LoopType;
        public Vector3 RotationTarget;
        public Transform ReturnTarget;
        public float ReturnSpeed;
        private CancellationTokenSource _internalCts;
        private readonly HashSet<uint> _interacted = new();

        protected bool _isReturning;
        private MotionHandle _handle;

        public override InteractionResultMeta Accept(EntityDamagable damageInteraction, ref InteractionCallMeta meta)
        {
            if (_interacted.Contains(damageInteraction.Uid)) return StaticInteractionResultMeta.Default;
            _interacted.Add(damageInteraction.Uid);
            return base.Accept(damageInteraction, ref meta);
        }

        public override void Release()
        {
            _handle.IsActiveCancel();
            if (Particle)
            {
                Particle.ParticleSystem.Stop(false);
            }
            if (ReleaseDelay > 0 && LastInteraction == LastInteractionType.Environment)
            {
                DelayRelease().Forget();
            }
            else
            {
                Pool.Return(this);
            }
        }

        public override void ProjectileUpdate()
        {
            if(!_isReturning) return;
            if (!ReturnTarget)
            {
                _currentLink.LifeTime = 0;
                return;
            }
            var delta = (ReturnTarget.position - transform.position);
            _currentLink.Controller.Velocity = delta.normalized * ReturnSpeed;
            if(delta.sqrMagnitude>1) return;
            _currentLink.LifeTime = 0;
        }

        public override void HandleHit(
            RaycastHit hit,
            out bool isBlocking)
        {
            base.HandleHit(hit, out isBlocking);
            if (!isBlocking) return;
            _isReturning = true;
            isBlocking = false;
        }
		
        private void OnDisable()
        {
            _handle.IsActiveCancel();
        }

        public override void OnRent()
        {
            base.OnRent();
            Trail.Clear();
            Rotate();
            _isReturning = false;
            _interacted.Clear();
        }

        public override void SetLink(ProjectileLink link)
        {
            CreateInternalCts();
            base.SetLink(link);
            AsyncLifetime(link.LifeTime,_internalCts.Token).Forget();
            link.LifeTime *= 10;
        }

        private void CreateInternalCts()
        {
            if (_internalCts is null || _internalCts.IsCancellationRequested)
            {
                _internalCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                return;
            }
            _internalCts?.Cancel();
            _internalCts?.Dispose();
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        private async UniTaskVoid AsyncLifetime(float time,CancellationToken ct)
        {
            await UniTask.Delay(time.ToSec(), cancellationToken: ct);
            _isReturning = true;
        }

        [Button]
        private void Stop()
        {
            KnifeModel.localEulerAngles = Vector3.zero;
            _handle.IsActiveCancel();
        }
		
        [Button]
        private void Rotate()
        {
            if(RotationTime<=0.001f) return;
            Stop();
			
            _handle = LMotion
                .Create(Vector3.zero, RotationTarget, RotationTime)
                .WithLoops(-1, LoopType)
                .BindToLocalEulerAngles(KnifeModel);
        }
    }
}