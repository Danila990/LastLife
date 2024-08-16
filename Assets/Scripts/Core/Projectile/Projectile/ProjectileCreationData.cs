using Core.Entity;
using UnityEngine;

namespace Core.Projectile.Projectile
{
    public readonly struct ProjectileCreationData
    {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly Vector3 ExtraVelocity;
        public readonly float Velocity;
        public readonly float LifeTime;
        public readonly EntityContext Source;
        public readonly string ProjectileKey;

        public ProjectileCreationData(
            Vector3 position,
            Vector3 direction,
            float velocity,
            EntityContext source,
            string projectileKey, 
            float lifeTime, 
            Vector3 extraVelocity = default
            )
        {
            Position = position;
            Direction = direction;
            Velocity = velocity;
            Source = source;
            ProjectileKey = projectileKey;
            LifeTime = lifeTime;
            ExtraVelocity = extraVelocity;
        }
    }
    
    public readonly struct ThrowableCreationData
    {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly Vector3 ExtraVelocity;
        public readonly float Velocity;
        public readonly EntityContext Source;
        public readonly string ThrowableKey;

        public ThrowableCreationData(
            Vector3 position,
            Vector3 direction,
            float velocity,
            EntityContext source,
            string throwableKey,
            Vector3 extraVelocity = default
        )
        {
            Position = position;
            Direction = direction;
            Velocity = velocity;
            Source = source;
            ThrowableKey = throwableKey;
            ExtraVelocity = extraVelocity;
        }
    }
}