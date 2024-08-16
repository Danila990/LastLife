using System.Collections.Generic;
using Core.Entity.Characters;
using Core.Entity.EntityAnimation;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
    public class C4WeaponContext : ThrowableWeaponContext
    {
        [SerializeField] private AnimationClip _tpvDetonateAnim;
        [SerializeField] protected float _tpvDetonateTrigger;
        private readonly List<RemoteExplosionEntity> _explosions = new();
        private bool _isDetonating;
        private CharacterAnimatorAdapter _animatorAdapter;
        private DoubleHandItemAnimator _doubleHandItemAnimator;

        protected override void OnCreated(IObjectResolver resolver)
        {
            base.OnCreated(resolver);
            if (Owner is CharacterContext characterContext)
            {
                _animatorAdapter = characterContext.CurrentAdapter.CharacterAnimatorAdapter;
            }
            
            if (ItemAnimator is DoubleHandItemAnimator doubleHandItemAnimator)
            {
                _doubleHandItemAnimator = doubleHandItemAnimator;
            }
        }

        private void OnDestroy()
        {
            Explode();
        }

        protected override void CreateItem()
        {
            if (HasQuantity && CurrentQuantity.Value <= 0) return;
            var data = GetItemData();
            var obj = ThrowableFactory.CreateObject(ref data);
            if (obj is RemoteExplosionEntity entity)
            {
                _explosions.Add(entity);
                entity.OnDestroyedCommand.Subscribe(OnExplosionDestroyed).AddTo(entity);
            }
            IgnoreCollisionAsync(obj, obj.destroyCancellationToken).Forget();
        }

        protected override void QuantityChange()
        {
            if(!HasQuantity) 
                return;
            
            _doubleHandItemAnimator.ExtraModel.SetActive(CurrentQuantity.Value > 1);
            
            if(CurrentQuantity.Value <= 0) 
                return;
            CurrentQuantity.Value--;
            
            if (CurrentQuantity.Value > 0)
                return;
            
            if (_explosions.Count > 0)
                return;
            
            if (Owner is CharacterContext context)
            {
                context.Inventory.RemoveItem(ItemId);
            }
        }

        public override string GetTossAnim()
        {
            return "c4toss";
        }

        public override string GetTossEventKey()
        {
            return "TossC4";
        }
        
        public void ExplodeRemote()
        {
            AsyncExplode().Forget();
        }

        private async UniTaskVoid AsyncExplode()
        {
            if(_isDetonating) 
                return;
            
            _isDetonating = true;
            var res = await _animatorAdapter.PlayAction(_tpvDetonateAnim,"Detonate","DetonatePress", _tpvDetonateTrigger);
            _isDetonating = false;
            
            if(!res)
                return;
            
            Explode();
            
            if (CurrentQuantity.Value > 0) 
                return;
            
            if (Owner is CharacterContext context)
            {
                context.Inventory.RemoveItem(ItemId);
            }
        }


        private void Explode()
        {
            for (var i = _explosions.Count - 1; i >= 0; i--)
            {
                _explosions[i].RemoteDetonate();
            }
        }

        private void OnExplosionDestroyed(RemoteExplosionEntity entity)
        {
            _explosions.Remove(entity);
        }
    }
}