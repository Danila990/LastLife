using Core.Entity;

namespace Core.Projectile.Projectile
{
    public class ProjectileLink
    {
        public readonly ProjectileCreationData CreationData;
        public readonly ProjectileEntity View;
        public readonly EntityContext Source;
        public ProjectileController Controller;
        public float LifeTime;
        public float InitialLifeTime;
        public bool FrameSkip = false;
        public bool PosUpdated = false;
        public bool Destroyed { get; set; }

        public ProjectileLink(
            ProjectileCreationData creationData,
            ProjectileEntity view,
            ProjectileController controller,
            EntityContext source
            )
        {
            CreationData = creationData;
            Controller = controller;
            Source = source;
            View = view;
            controller.PrevPos = creationData.Position;
        }
    }
}