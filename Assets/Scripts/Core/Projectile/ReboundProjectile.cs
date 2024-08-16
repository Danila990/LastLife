using AnnulusGames.LucidTools.Audio;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using UnityEngine;

namespace Core.Projectile
{
    public class ReboundProjectile : SimpleProjectile
    {
        [SerializeField] private AudioClip _reboundSound;
        [SerializeField] private int _maxRebounds = 0;
        private int _currentRebounds;
        protected bool LastEnverCollide;
        
        public override InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
        {
            var res = base.Accept(environment, ref meta);
            res = Rebound(ref res, ref meta);
            return res;
        }

        protected InteractionResultMeta Rebound(ref InteractionResultMeta result, ref InteractionCallMeta meta)
        {
            if (_currentRebounds > 0)
            {
                var dot = Vector3.Dot(-CurrentLink.Controller.Velocity.normalized, meta.Normal);
                if (dot > 0.4f)
                {
                    LastEnverCollide = true;
                    return result;
                }
                CurrentLink.Controller.Velocity = Vector3.Reflect(CurrentLink.Controller.Velocity * (1-dot), meta.Normal);
                _currentRebounds--;
                LucidAudio.PlaySE(_reboundSound).SetPosition(transform.position).SetSpatialBlend(1f);
                return StaticInteractionResultMeta.InteractedPassed;
            }
            LastEnverCollide = true;
            return result;
        }
        
        public override void Release()
        {
            base.OnRent();

            if (ReleaseDelay > 0 && LastEnverCollide)
            {
                DelayRelease().Forget();
            }
            else
            {
                Pool.Return(this);
            }
        }
        
        public override void OnRent()
        {
            base.OnRent();
            _currentRebounds = _maxRebounds;
            LastEnverCollide = false;
        }
    }


}