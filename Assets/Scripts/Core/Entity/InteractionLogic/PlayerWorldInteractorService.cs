using System;
using Core.Entity.InteractionLogic.Interactions;
using Core.InputSystem;
using Core.Services.Input;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer.Unity;

namespace Core.Entity.InteractionLogic
{
    public class PlayerWorldInteractorService : IStartable, IDisposable, IInteractorVisiter, ITickable
    {
        private readonly IRayCastService _rayCastService;
        private readonly PlayerInputView _playerInputView;
        private readonly IInputService _inputService;
        private readonly IPlayerSpawnService _playerSpawnService;
        private readonly CompositeDisposable _compositeDisposable = new();
        private PlayerInputInteraction _currentInteraction;
        private bool _isPressed;
        private float _timePassed;
        private float _currentDist;
        
        
        public PlayerWorldInteractorService(
            IRayCastService rayCastService,
            PlayerInputView playerInputView,
            IInputService inputService,
            IPlayerSpawnService playerSpawnService
        )
        {
            _rayCastService = rayCastService;
            _playerInputView = playerInputView;
            _inputService = inputService;
            _playerSpawnService = playerSpawnService;
        }
        
        public void Start()
        {
            //_rayCastService.CurrentHitObservable.Subscribe(OnChangeHit).AddTo(_compositeDisposable);
            //_inputService.ObserveGetButton(InputConsts.INTERACT).Subscribe(OnInput).AddTo(_compositeDisposable);
            //_playerInputView.InteractionButton.gameObject.SetActive(false);
            //_playerInputView.InteractionFiller.gameObject.SetActive(false);
        }
        
        public void Tick()
        {
            if (!_isPressed) return;
            if (_currentInteraction.TimeToUse <= 0)
            {
                UseInteract();
                return;
            }

            _timePassed += Time.deltaTime;
            //_playerInputView.InteractionFiller.fillAmount = _timePassed / _currentInteraction.TimeToUse;
            
            if (_timePassed >= _currentInteraction.TimeToUse)
            {
                UseInteract();
            }
        }

        private void UseInteract()
        {
            _currentInteraction.Use(_playerSpawnService.PlayerCharacterAdapter.CurrentContext);
            _timePassed = 0;
            _isPressed = false;
            //_playerInputView.InteractionFiller.fillAmount = 0;
            OnChangeHit(_rayCastService.CurrentHit);
        }

        private void OnInput(bool state)
        {
            if(!_currentInteraction && state) return;
            _isPressed = state;
            if (!state)
            {
               // _playerInputView.InteractionFiller.fillAmount = 0;
                _timePassed = 0;
            }
        }

        private void OnHitMiss(Unit _)
        {
            Reset();
        }

        private void Reset()
        {
            if(_isPressed) return;
            _currentInteraction = null;
            //_playerInputView.InteractionButton.gameObject.SetActive(false);
            //_playerInputView.InteractionFiller.gameObject.SetActive(false);
            //_playerInputView.InteractionFiller.fillAmount = 0;
        }
        
        private void OnChangeHit(RaycastHit hit)
        {
            if (hit.transform)
            {
                _currentDist = hit.distance;
            }
            
            if(_inputService.GetButton(InputConsts.INTERACT))
                return;
            
            Reset();
            if(!hit.transform) return;
            _currentDist = hit.distance;
            _rayCastService.RayInteract(this);
        }
        
        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) {return StaticInteractionResultMeta.Default;}
        public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) {return StaticInteractionResultMeta.Default;}
        public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) {return StaticInteractionResultMeta.Default;}
        public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta) {return StaticInteractionResultMeta.Default;}
        public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) {return StaticInteractionResultMeta.Default;}
        public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

        public InteractionResultMeta Accept(PlayerInputInteraction interaction, ref InteractionCallMeta meta)
        {
            if (interaction.InteractDistance <= _currentDist) return StaticInteractionResultMeta.InteractedBlocked;
            //_playerInputView.InteractionButton.gameObject.SetActive(true);
            //_playerInputView.InteractionFiller.gameObject.SetActive(true);
            _currentInteraction = interaction;
            return StaticInteractionResultMeta.InteractedBlocked;
        }
    }
}