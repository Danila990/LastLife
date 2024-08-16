using Core.Entity.InteractionLogic.Interactions;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
    public interface ISpecialExplosionHandler
    {
        void Handle();
    }
    
    public interface ISpecialExplosionSource
    {
    }

    public class RemoteExplosionEntity : ExplosionEntity, ISpecialExplosionSource
    {
        private ReactiveCommand<RemoteExplosionEntity> _onDestroyedCommand;
        public IReactiveCommand<RemoteExplosionEntity> OnDestroyedCommand => _onDestroyedCommand;

        protected override void OnCreated(IObjectResolver resolver)
        {
            _onDestroyedCommand = new ReactiveCommand<RemoteExplosionEntity>().AddTo(this);
        }

        protected override void OnUpdate() { }

        protected override void OnCollisionEnter(Collision other) { }
        
        public void RemoteDetonate()
        {
            ExplosionVFX();
        }

        protected override void OnAccept(EntityDamagable damage, Vector3 metaPoint)
        {
            if (damage is ISpecialExplosionHandler handler)
            {
                handler.Handle();
            }
        }

        protected virtual void OnDestroy()
        {
            _onDestroyedCommand.Execute(this);
        }
    }
}