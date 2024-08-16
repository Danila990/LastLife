using Cysharp.Threading.Tasks;

namespace Core.Inventory.Items.Weapon
{
    public class MinigunWeaponContext : SimpleProjectileWeaponContext
    {
        public override void Shoot()
        {
            AsyncShoot();
            base.Shoot();
        }

        private async UniTaskVoid AsyncShoot()
        {
            var origin = GetOrigin();
            if (origin.transform.TryGetComponent<IMetaOrigin>(out var meta))
            {
                await meta.StartAnim();
            }

            meta?.EndAnim().Forget();
        }
    }
}