using System;
using Core.Entity;
using UniRx;
using UnityEngine;

namespace Core.HealthSystem
{
    [Serializable]
    public class Health : IHealth, IDisposable
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _currentHealth;
        protected ReactiveCommand<DiedArgs> _onDeath;
        protected ReactiveCommand<DamageArgs> _onDamage;
        protected BoolReactiveProperty _canDamage;
        [field: SerializeField] public bool IsDeath { get; protected set; }
        public bool IsImmortal => _canDamage is { Value: false };
        public float MaxHealth        {
            get => _maxHealth;
            protected set => _maxHealth = value;
        }
        public float CurrentHealth
        {
            get => _currentHealth;
            protected set => _currentHealth = value;
        }
        
        public float PercentHealth => _currentHealth / _maxHealth;
        public IReactiveCommand<DiedArgs> OnDeath => _onDeath;
        public IReactiveCommand<DamageArgs> OnDamage => _onDamage;
        
        public virtual void DoDamage(ref DamageArgs args)
        {
            if(!_canDamage.Value)
                return;
                
            CurrentHealth -= args.Damage;
            _onDamage.Execute(args);
            NotifyDeath(ref args);
        }

        public virtual void SetCurrentHealth(float amount)
        {
            if (IsImmortal)
                return;
            
            _currentHealth = amount;
            var arg = new DamageArgs(null);
            _onDamage.Execute(arg);
            NotifyDeath(ref arg);
        }

        public void Resurrect(float procent)
        {
            IsDeath = false;
            SetCurrentHealth(_maxHealth * procent);
        }

        private void NotifyDeath(ref DamageArgs args)
        {
            if (_currentHealth > 0) return;
            if (IsDeath) return;
            IsDeath = true;
            OnDiedEvent(ref args);
        }

        public void ForceDeath()
        {
            if (IsDeath || IsImmortal) 
                return;
            IsDeath = true;
            var arg = new DamageArgs(null);
            OnDiedEvent(ref arg);
        }

        protected virtual void OnDiedEvent(ref DamageArgs args)
        {
            _onDeath?.Execute(new DiedArgs(null, args.DamageSource,args.MetaDamageSource));
        }

        public void Init()
        {
            _canDamage = new BoolReactiveProperty(true);
            _onDeath = new ReactiveCommand<DiedArgs>(_canDamage);
            _onDamage = new ReactiveCommand<DamageArgs>(_canDamage);
            OnInit();
        }
        
        public void SetImmortal(bool immortal)
        {
            _canDamage.Value = !immortal;
        }

        protected virtual void OnInit()
        {
            
        }

        public void Dispose()
        {
            _onDeath?.Dispose();
            _onDamage?.Dispose();
            _canDamage?.Dispose();

            OnDispose();
        }

        public void SetMaxHealth(float value)
        {
            _maxHealth = value;
        }
        
        protected virtual void OnDispose(){}
    }

    public readonly struct DiedArgs
    {
        public readonly EntityContext SelfEntity;
        public readonly EntityContext DiedFrom;
        public readonly IDamageSource MetaDamage;

        public DiedArgs(EntityContext selfEntity, EntityContext diedFrom, IDamageSource metaDamage)
        {
            SelfEntity = selfEntity;
            DiedFrom = diedFrom;
            MetaDamage = metaDamage;
        }
    }
}