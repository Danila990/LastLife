using System;
using System.Threading;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Inventory.Items.Weapon;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
    [CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ThrowingAction), fileName = nameof(ThrowingAction))]
    public class ThrowingAction : GenericEntityAction<ThrowableWeaponContext>
    {
        [Inject] private readonly ICameraService _cameraService;
        private bool _isWaiting;
        private CharacterContext _characterContext;

        public override void OnDeselect()
        {
            if (CurrentContext)
                CurrentContext.Use(false);
        }
        public override void OnInput(bool state)
        {
            Settings[CurrentContext.Uid].LastInput = state;
        }
		
        public override void OnInputUp()
        {
            CurrentContext.Use(false);
        }
		
        public override void OnInputDown()
        {
            if (_characterContext.CurrentAdapter.AimController.IsAiming.Value || !_cameraService.IsThirdPerson)
            {
                CurrentContext.Use(true);
                return;
            }
			
            WaitAiming(() => CurrentContext.Use(true), _characterContext.destroyCancellationToken).Forget();
        }
        
        private async UniTaskVoid WaitAiming(Action callback, CancellationToken token)
        {
            if (_isWaiting)
                return;
            _isWaiting = true;
            if (_cameraService.IsThirdPerson)
            {
                await _characterContext.CurrentAdapter.AimController.IsAiming.WaitUntilValueChangedAsyncUniTask(cancellationToken: token);
            }
            _isWaiting = false;
            if (Settings[CurrentContext.Uid].LastInput)
            {
                callback();
            }
        }
        
        public override void SetContext(EntityContext context)
        {
            base.SetContext(context);
            _characterContext = CurrentContext.Owner as CharacterContext;
            _isWaiting = false;
        }
    }
}