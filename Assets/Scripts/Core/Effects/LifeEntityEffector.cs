using Core.Entity;
using Core.HealthSystem;
using UniRx;

namespace Core.Effects 
{
    public abstract class LifeEntityEffector : BaseEntityEffector
    {
        private LifeEntity _lifeEntity;

        public virtual void SetContext(LifeEntity context)
        {
            _lifeEntity = context;
            context.Health.OnDeath.Subscribe(OnDeath).AddTo(context);
        }

        public override void DoEffect(EffectArgs args)
        {
            if (_lifeEntity.Health.IsDeath)
                return;

            base.DoEffect(args);
        }

        private void OnDeath(DiedArgs _)
        {
            RemoveAllEffects();
        }

        protected abstract override (string, Effect) DoEffectInternal(EffectArgs args);
    }
}