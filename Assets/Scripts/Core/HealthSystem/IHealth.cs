using UniRx;
using UnityEngine;

namespace Core.HealthSystem
{
    public interface IHealth
    {
        float MaxHealth { get; }
        float CurrentHealth { get; }
        float PercentHealth { get; }
        bool IsDeath { get; }
        bool IsImmortal { get; }
        IReactiveCommand<DiedArgs> OnDeath { get; }
        IReactiveCommand<DamageArgs> OnDamage { get; }
        void DoDamage(ref DamageArgs args);
        void ForceDeath();
        void SetCurrentHealth(float amount);
        void Resurrect(float percent);
    }

    public interface ILifeEntityHealth : IHealth
    {
        bool DiedFromBloodLoss { get; set; }
        float CurrentBloodLevel { get; }
        void StartBloodLoss(ref DamageArgs args, Vector3 pos, Vector3 normal, Transform attach);
        void AddHealth(float amount);
        public void AddHealthWithoutClamp(float value);
        public void ClampHealth();
        void AddHealthPercent(float percent);
        public void SetProxyHealth(IHealth proxyHealth);
        public void RemoveProxyHealth(IHealth proxyHealth);
    }
}