using UnityEngine;

namespace Core.Projectile.Projectile
{
    public struct ProjectileController
    {
        public Vector3 Velocity;
        public Vector3 Position;
        public Vector3 PrevPos;
        public float Mass;

        public ProjectileController(Vector3 velocity, Vector3 position,Vector3 startPos,float mass)
        {
            Position = position;
            Velocity = velocity;
            PrevPos = startPos;
            Mass = mass;
        }
    }
}