
using UnityEngine;

namespace Core.Projectile
{
    public class FollowSaiProjectile : SteelFanProjectile
    {
        private IProjectileTarget _target;
        private float _initialSpeed;
        private float _additionalStabilty;
        
        public override void ProjectileUpdate()
        {
            base.ProjectileUpdate();
            if (_target is null) return;
            if (_isReturning) return;
            var delta = _target.GetTarget() - _currentLink.View.transform.position;
            var oldDir = _currentLink.Controller.Velocity.normalized;
            var newDir = delta.normalized;
            Debug.Log(_additionalStabilty);
            var dir = Vector3.RotateTowards(oldDir, newDir, (20 + _additionalStabilty) * Mathf.Deg2Rad * Time.deltaTime,0).normalized;
            _currentLink.Controller.Velocity = dir * _initialSpeed;
            //_currentLink.View.transform.rotation = Quaternion.LookRotation(dir);
        }

        public void SetTarget(IProjectileTarget target)
        {
            _target = target;
            _initialSpeed = _currentLink.Controller.Velocity.magnitude;
        }

        public override void OnRent()
        {
            base.OnRent();
            _additionalStabilty = 0;
        }

        public void SetStability(float value)
        {
            _additionalStabilty = value;
        }
    }
    
    public interface IProjectileTarget
    {
        public Vector3 GetTarget();
    }
}