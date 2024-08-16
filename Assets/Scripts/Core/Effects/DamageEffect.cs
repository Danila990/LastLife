using Core.Entity;
using Core.HealthSystem;
using System;
using UniRx;

namespace Core.Effects
{
    public class DamageEffect
    {
        private Action OnEnd;
        private readonly float _damage;
        private readonly int _countTick;
        private readonly float _tickDelay;
        private readonly IHealth _health;

        private IDisposable _disposable;

        public DamageEffect(float damage, int countTick, float tickDelay, IHealth health, Action action = null)
        {
            _damage = damage;
            _countTick = countTick;
            _tickDelay = tickDelay;
            _health = health;
            OnEnd = action;
        }

        public void Renew()
        {
            _disposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(_tickDelay))
            .Take(_countTick).DoOnCompleted(OnEnd)
            .Subscribe(_ => TickDamage());
        }


        private void TickDamage()
        {
            _health.SetCurrentHealth(_health.CurrentHealth - _damage);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}