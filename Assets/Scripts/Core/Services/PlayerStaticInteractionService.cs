using System;
using Core.Actions;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using Ui.Sandbox.PlayerInput;
using UniRx;
using Utils;

namespace Core.Services
{
	public class PlayerStaticInteractionService : IPlayerStaticInteractionService, IPlayerInputListener
	{
		private readonly IPlayerInputProvider _playerInputProvider;
		private readonly PlayerInputController _playerInputController;
		private readonly InstallerCancellationToken _installerCancellationToken;
		private StaticWeaponInteraction _staticWeaponInteraction;
		private IDisposable _disposable;

		public PlayerStaticInteractionService(
			IPlayerInputProvider playerInputProvider, 
			PlayerInputController playerInputController,
			InstallerCancellationToken installerCancellationToken)
		{
			_playerInputProvider = playerInputProvider;
			_playerInputController = playerInputController;
			_installerCancellationToken = installerCancellationToken;
		}
		
		public void Attach(
			StaticWeaponInteraction staticWeaponInteraction, 
			IReactiveCommand<DiedArgs> healthDeath)
		{
			_disposable?.Dispose();
			_staticWeaponInteraction = staticWeaponInteraction;
			_playerInputController.SwitchInputRig(InputRigType.StaticItemRig);
			_playerInputController.ActiveInputRig.Value.OnSelectedItemChanged(staticWeaponInteraction.StaticItemContext);
			_disposable = healthDeath.Subscribe(OnContextDied);
			ActivateInput().Forget();
		}
		
		private void OnContextDied(DiedArgs obj)
		{
			Detach();
		}

		private async UniTaskVoid ActivateInput()
		{
			await UniTask.DelayFrame(4, cancellationToken: _installerCancellationToken.Token);
			_playerInputProvider.UseListener(this);
		}
		
		public void Detach()
		{
			_disposable?.Dispose();
			if (_staticWeaponInteraction)
			{
				_staticWeaponInteraction.Detach();
				_playerInputController.SwitchInputRig(InputRigType.PlayerInputRig);

				_staticWeaponInteraction = null;
				_disposable = null;
			}
		}

		public PlayerInputDto InputDto { get; } = new PlayerInputDto();
		public void OnAction(ActionKey action, InputKeyType keyType)
		{
			if(_staticWeaponInteraction is {StaticItemContext: null})
				return;

			switch (keyType)
			{
				case InputKeyType.GetDown:
					_staticWeaponInteraction.StaticItemContext.InputDown(action);
					break;
				case InputKeyType.GetUp:
					_staticWeaponInteraction.StaticItemContext.InputUp(action);
					break;
			}
		}
		
		public void OnActionGet(ActionKey action, bool status)
		{
			_staticWeaponInteraction.StaticItemContext.OnInput(action, status);
		}
		
		public void OnSprintDown()
		{
			
		}
		
		public void OnJumpDown()
		{
		}
		public void OnJumpUp()
		{
		}
		public void OnMoveDown()
		{
			
		}
		
		public void OnAimDown()
		{
		}
	}
    
    public interface IPlayerStaticInteractionService
    {
	    void Attach(StaticWeaponInteraction staticWeaponInteraction, IReactiveCommand<DiedArgs> healthDeath);
	    void Detach();
    }
}