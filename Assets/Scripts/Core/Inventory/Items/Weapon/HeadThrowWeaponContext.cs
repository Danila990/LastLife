using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
    public class HeadThrowWeaponContext : SimpleProjectileWeaponContext
    {
        [Inject] private IRayCastService _rayCastService;
        private bool _isActive;
        
        public override void Shoot()
        {
            AsyncShoot().Forget();
        }

        private async UniTaskVoid AsyncShoot()
        {
            if(_isActive) return;
            _isActive = true;
            var origin = GetOrigin();
            var dir = GetDir(origin);

            if (origin.transform.TryGetComponent<IMetaOrigin>(out var meta))
            {
                await meta.StartAnim();
            }
            
            var inaccuracy = GetInaccuracy(origin);
            var pos = origin.position;
            if (meta is not  null)
            {
                pos = meta.OriginPos;
            }
            var link = CreateProjectile(pos, dir, inaccuracy);
            if (link.View is SteelFanProjectile prj)
            {
                prj.ReturnTarget = GetOrigin();
            }
            if (link.View is FollowSaiProjectile prj1)
            {
                prj1.ReturnTarget = GetOrigin();
                prj1.SetTarget(GetTarget());
                prj1.SetStability(GetValue(WeaponConsts.SPIN));
            }
            OnShoot();

            await UniTask.WaitUntil(() => link.Destroyed);
            
            meta?.EndAnim().Forget();
            _isActive = false;
        }

        private IProjectileTarget GetTarget()
        {
            if (Owner is not CharacterContext ctx) return new AiProjectileTarget(GetOrigin());
            if (ctx.Adapter is PlayerCharacterAdapter)
            {
                return new PlayerProjectileTarget(_rayCastService);
            }
            return new AiProjectileTarget(GetOrigin());
        }
    }

    public class AiProjectileTarget : IProjectileTarget
    {
        private readonly Transform _origin;

        public AiProjectileTarget(Transform origin)
        {
            _origin = origin;
        }
        
        public Vector3 GetTarget()
        {
            if(!_origin) return Vector3.zero;
            return _origin.position + _origin.forward * 100;
        }
    }

    public class PlayerProjectileTarget : IProjectileTarget
    {
        private readonly IRayCastService _rayCastService;

        public PlayerProjectileTarget(IRayCastService rayCastService)
        {
            _rayCastService = rayCastService;
        }

        public Vector3 GetTarget()
        {
            return _rayCastService.CurrentHitPoint;
        }
    }
}