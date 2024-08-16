using AnnulusGames.LucidTools.Audio;
using Core.Effects;
using Core.Entity.Characters;
using Core.Entity.Repository;
using Core.HealthSystem;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity
{
    public class PhysicEntityContext : EntityContext
    {
        [ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")] [SerializeField]
        private string VFXKey;
        [SerializeField] private AudioClip _deathSound;
        public Health _health;
        [SerializeField] private bool _isEffectedObject = false;
        [SerializeField, ShowIf(nameof(_isEffectedObject))] private EnvironmentEntityEffector _effector;
        private DamageEffectHandler _damageEffectHandler;

        [Inject] private readonly IEntityRepository _entityRepository;
        
        protected override void OnCreated(IObjectResolver resolver)
        {
            _health.Init();
            _health.OnDeath.Subscribe(OnDeath).AddTo(this);
            if (!_isEffectedObject)
                return;

            _effector ??= GetComponent<EnvironmentEntityEffector>();
            _effector.SetFxFactory(VFXFactory);
            _effector.Init();
            _effector.SetHealth(_health);
            _damageEffectHandler = new DamageEffectHandler();
            _damageEffectHandler.Init(_effector);
            _damageEffectHandler.SetContext(_effector.EffectsDataSo, _health, transform);
        }

        private void OnDeath(DiedArgs _)
        {
            if (_isEffectedObject)
                _effector.RemoveAllEffects();

            if (!string.IsNullOrEmpty(VFXKey))
                VFXFactory.CreateAndForget(VFXKey, transform.position);
            
            if (_deathSound)
            {
                LucidAudio
                .PlaySE(_deathSound)
                .SetPosition(transform.position)
                .SetSpatialBlend(1);
            }
            
            DestroySelf();
        }
        
        private void DestroySelf()
        {
            OnDestroyed(_entityRepository);
            Destroy(gameObject);
        }

        public override void DoDamage(ref DamageArgs args, DamageType type)
        {
            _health.DoDamage(ref args);
        }

        public override void DoEffect(ref EffectArgs args)
        {
            _effector.DoEffect(args);
        }
    }

}