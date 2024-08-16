using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic;
using Core.Factory;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameSettings;
using SharedUtils;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
    public class ParabolicWeaponContext : SimpleProjectileWeaponContext
    {
        [Inject] protected readonly IRayCastService RayCastService;
        [Inject] protected readonly IProjectileFactory ProjectileFactory;

        public override void Shoot()
        {
#if UNITY_EDITOR
            if (_isDebug)
            {
                GetOrigin();
                var forward = GetOrigin().forward;
                var position = GetOrigin().position;
                
                var firstCircle = position + Quaternion.LookRotation(forward) * new Vector3(0, 0, 1);
                Util.DrawCircle(firstCircle, Quaternion.LookRotation(forward), _inaccuracy, 12, Color.cyan, 1);
                var nextCirclePos = position + Quaternion.LookRotation(forward) * new Vector3(0, 0, 5);
                Util.DrawCircle(nextCirclePos, Quaternion.LookRotation(forward), _inaccuracy * 5, 12, Color.cyan, 1);
            }
#endif

            AsyncShoot();
            
            if(!_infinityClip)
                _currentClip.Value--;
        }

        private async UniTaskVoid AsyncShoot()
        {
            var origin = GetOrigin();

            if (origin.transform.TryGetComponent<IMetaOrigin>(out var meta))
            {
               await meta.StartAnim();
            }
            
            for (var i = 0; i < _shootCount; i++)
            {
                var inaccuracy = GetInaccuracy(origin);
                var mass = ProjectileFactory.GetProjectileMass(WeaponData.ProjectileKey);
                var dir = Calc(origin.position,GetTargetPosition(),mass);
                Util.DrawSphere(GetTargetPosition(),Quaternion.identity, 1,Color.yellow,1);
                var pos = origin.position;
                
                if (meta is not  null)
                {
                    pos = meta.OriginPos;
                }
                var link = CreateProjectile(pos, dir, inaccuracy);
                link.Controller.Mass = mass;
            }
            OnShoot();

            meta?.EndAnim().Forget();
        }
        
        public Vector3 GetTargetPosition()
        {
            var ray = RayCastService.CurrentHitPoint - RayCastService.RayPosition;
            var normal = RayCastService.CurrentNormal;
            
            normal.x = 0;
            normal.z = 0;
            normal.y = Mathf.Min(0, normal.y * 1.5f);
            var result = Vector3.ClampMagnitude(ray, 25 * GetValue(WeaponConsts.FALLOFF)) + RayCastService.RayPosition +
                         normal;
            if (Owner is not CharacterContext context) return result;
            if (context.Adapter is not AiCharacterAdapter ai) return result;
            return ai.CurrentTarget?.LookAtPoint ?? result;
        }
        
        private static Vector3 Calc(Vector3 startPos, Vector3 targetPos,float mass)
        {
            var maxH = Mathf.Max(startPos.y, targetPos.y) + 1;
            var h1 = maxH - startPos.y;
            var h2 = maxH - targetPos.y;
#if UNITY_EDITOR
            var g = 9.81f / (mass * 2f);
#else 
            var g = 9.81f / (mass * 2);
#endif
            var velY = Mathf.Sqrt(2 * h1 * g);
            var t1 = velY / g;
            var t2 = Mathf.Sqrt(2 * h2 / g);
            var t = t1 + t2;
            var delta = targetPos - startPos;
            delta.y = 0;
            var velXZ = delta / t;
            return velY  * Vector3.up + velXZ;
        }
        
        protected override void OnShoot()
        {
            if (_vfxContext)
            {
                var origin = GetOrigin();
                _vfxContext.transform.SetParent(origin);
                _vfxContext.ParticleSystem.transform.SetPositionAndRotation(origin.position, origin.rotation);
                _vfxContext.ParticleSystem.transform.localScale = IsTpvMode ? _muzzleTPVScale : _muzzleFPVScale;
                if (!GameSetting.ViolenceStatus)
                {
                    _vfxContext.Play();
                }
            }

            if (ShootSound)
            {
                LucidAudio
                    .PlaySE(ShootSound)
                    .SetPosition(ShootingOrigin.GetStaticOrigin().position)
                    .SetVolume(_shootSoundVolume)
                    .SetSpatialBlend(1);
            }
            

            if (ItemAnimator)
            {
                ItemAnimator.PlayAnim("Shoot");
            }
        }
    }
}