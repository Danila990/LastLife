using System;
using Core.CameraSystem;
using Core.Entity.Ai;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.EntityAnimation;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Inventory;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using FIMSpace.FProceduralAnimation;
using SharedUtils;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity
{
    public class MechEntityContext : LifeEntity, IMeleeCharacter, IControllableEntity
    {
        [SerializeField] private CharacterHealth _health;
        [SerializeField] private LegsAnimator _legsAnimator;
        [SerializeField] private BaseInventory _inventory;
        [SerializeField] private GenericInteraction _interaction;
        [SerializeField] private MonoBehaviour[] _toInject;
        [SerializeField] private Transform _lookAtTransform;
        [field:SerializeField] public Transform SeatTransform { get; set; }
        public CharacterAnimator CharacterAnimator;
        public IInventory Inventory => _inventory;
        public override ILifeEntityHealth Health => _health;
        public override Transform LookAtTransform => _lookAtTransform;
        public CharacterAnimatorAdapter AnimatorAdapter => _currentAdapter.CharacterAnimatorAdapter;
        public StatsProvider StatsProvider => _currentAdapter.StatsProvider;
        public CharacterAnimator CharacterAnimatorRef => CharacterAnimator;
        public IEntityAdapter Adapter => _currentAdapter;
        
        private MechCharacterAdapter _currentAdapter;
        private EntityTarget _selfTarget;

        protected override void OnCreated(IObjectResolver resolver)
        {
            _inventory.Initialize(this,resolver);
            _health.Init();
            AdditionalCameraDistance = 5;
            Enable().Forget();
            foreach (var target in _toInject)
            {
                resolver.Inject(target);
            }

            _interaction.Used.Subscribe(OnSeat).AddTo(this);
        }

        private void OnSeat(CharacterContext context)
        {
            _currentAdapter.OnSeat(context);
            _interaction.SetDontShow(true);
        }

        public void OnLeave()
        {
            _interaction.SetDontShow(false);
        }


        
        private async UniTask Enable()
        {
            _legsAnimator.enabled = false;
            await UniTask.Delay(.35f.ToSec(), cancellationToken: destroyCancellationToken);
            _legsAnimator.enabled = true;
        }

        public void SetAdapter(MechCharacterAdapter adapter)
        {
            _currentAdapter = adapter;
        }
        
        public override bool TryGetAiTarget(out IAiTarget result)
        {
            if (_health.IsDeath)
            {
                result = null;
                return false;
            }
            else
            {
                _selfTarget ??= new EntityTarget(this);
                result = _selfTarget;
                return true;
            }
        }


    }
}