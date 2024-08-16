namespace Core.Projectile.Projectile
{
    public interface IProjectileService
    {
        void CreateProjectile(ProjectileCreationData creationData);
        void CreateProjectile(ProjectileCreationData creationData, out ProjectileLink projectileLink);
        void ReleaseProjectile(ProjectileLink projectileLink);
    }
}