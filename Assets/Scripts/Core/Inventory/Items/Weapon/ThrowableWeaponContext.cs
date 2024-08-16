using System;
using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.Entity.InteractionLogic;
using Core.Factory;
using Core.Inventory.Items.Weapon.Adapters;
using Core.Inventory.Origins;
using Core.Projectile.Projectile;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
    public class ThrowableWeaponContext : WeaponContextWithAdapter<ThrowableAdapter>
    {
        [field:SerializeField] public ThrowableWeaponData WeaponData { get; set; }
        [SerializeField] private float _attackDelay;
        [SerializeField] private float _inaccuracy;
        [SerializeField] private AnimationClip _throwAnimation;
        [SerializeField] private float _animTriggerTime;
        [SerializeField] private AudioClip _throwSound;
        protected SimpleTimerAction TimerAction;
        public BaseOriginProvider ShootingOrigin { get; private set; }

        [Inject] protected readonly IThrowableFactory ThrowableFactory;
        [Inject] public readonly IRayCastService RayCastService;

        public AnimationClip ThrowAnimation => _throwAnimation;
        public float AnimTriggerTime => _animTriggerTime;
        public float AttackDelay => _attackDelay;
        
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            TimerAction.Tick(ref deltaTime);
        }

        protected override void OnCreated(IObjectResolver resolver)
        {
            base.OnCreated(resolver);
            TimerAction = new SimpleTimerAction(_attackDelay);
            TimerAction.SetAction(Throw);
        }

        public override void ItemInit(IOriginProxy inventory)
        {
            base.ItemInit(inventory);
            ShootingOrigin = inventory.Origin;
        }

        protected override ThrowableAdapter TryGetWeaponAdapter(EntityContext owner, IEntityAdapter adapter)
        {
            return owner switch
            {
                CharacterContext => new ThrowableCharacterAdapter(this).AddTo(this),
                SpiderHeadContext => new ThrowableSpiderAdapter(this).AddTo(this),
                _ => null
            };
        }

        public void Use(bool state)
        {
            if (HasQuantity)
            {
                TimerAction.CanUse(state && CurrentQuantity.Value > 0);
                return;
            }
            
            TimerAction.CanUse(state);
        }

        private void Throw()
        {
            WeaponAdapter.Throw();
        }

        protected virtual void CreateItem()
        {
            var data = GetItemData();
            var throwableEntity = ThrowableFactory.CreateObject(ref data);
            IgnoreCollisionAsync(throwableEntity, throwableEntity.destroyCancellationToken).Forget();
        }

        protected async UniTaskVoid IgnoreCollisionAsync(ThrowableEntity throwableEntity, CancellationToken token)
        {
            var layers = throwableEntity.TargetRigidbody.excludeLayers;
            layers.value = LayerMasks.CharacterLayers;
            throwableEntity.TargetRigidbody.excludeLayers = layers;
            await UniTask.Delay(.7f.ToSec(), cancellationToken: token);
            layers.value = 0;
            throwableEntity.TargetRigidbody.excludeLayers = layers;
        }

        protected ThrowableCreationData GetItemData()
        {
            var targetPos = WeaponAdapter.GetTargetPosition();
            var spawnPos = WeaponAdapter.GetSpawnOriginPos();

            Util.DrawSphere(targetPos, Quaternion.identity, 2,Color.yellow,1);
            
            var res = Calc(spawnPos, targetPos);
            return new ThrowableCreationData(
                spawnPos,
                res.normalized,
                res.magnitude,
                Owner,
                WeaponData.ThrowableKey
            );
        }

        private static Vector3 Calc(Vector3 startPos, Vector3 targetPos)
        {
            var maxH = Mathf.Max(startPos.y, targetPos.y) + 1;
            var h1 = maxH - startPos.y;
            var h2 = maxH - targetPos.y;
            const float g = 9.81f;
            var velY = Mathf.Sqrt(2 * h1 * g);
            var t1 = velY / g;
            var t2 = Mathf.Sqrt(2 * h2 / g);
            var t = t1 + t2;
            var delta = targetPos - startPos;
            delta.y = 0;
            var velXZ = delta / t;
            return velY  * Vector3.up + velXZ;
        }
        
        protected virtual void QuantityChange()
        {
            if(!HasQuantity) 
                return;
            
            CurrentQuantity.Value--;
            if (CurrentQuantity.Value > 0) 
                return;
            
            if (Owner is CharacterContext context)
            {
                context.Inventory.RemoveItem(ItemId);
            }
        }

        public void ThrowObject()
        {
            if(HasQuantity && CurrentQuantity.Value <= 0) 
                return;
            
            CreateItem();
            OnShoot();
            QuantityChange();
        }

        public virtual string GetTossAnim()
        {
            return "Grenade Toss";
        }

        public virtual string GetTossEventKey()
        {
            return "Toss";
        }

        protected virtual void OnShoot()
        {
            if (_throwSound)
            {
                LucidAudio.PlaySE(_throwSound).SetPosition(WeaponAdapter.GetSpawnOriginPos()).SetSpatialBlend(1);
            }
        }
    }
    
    [Serializable]
    public class ThrowableWeaponData
    {
        public float Velocity;
        public string ThrowableKey;
    }
}