using System;
using Core.Entity.Characters;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Ui.Sandbox.PlayerInput;
using UniRx;
using VContainer.Unity;

namespace Core.Carry
{
	public class CarryInventoryService : ICarryInventoryService, IStartable, IDisposable
	{
		private readonly PlayerInputController _playerInputController;
		private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;

		private IDisposable _disposable;
		private IDisposable _subscription;
		private CarryInventory _inventory;
		private CharacterContext _currentContext;
		
		public CarryInventoryService(PlayerInputController playerInputController, ISubscriber<PlayerContextChangedMessage> subscriber)
		{
			_playerInputController = playerInputController;
			_subscriber = subscriber;
		}

		public void Start()
		{
			_subscription = _subscriber.Subscribe(OnContextChanged);
		}

		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			_currentContext = msg.CharacterContext;
		}

		public void DetachCurrent()
		{
			if(!_currentContext)
				return;
			
			_currentContext.CarryInventory.Remove();
			Detach();
		}
		
		
		public void Attach(CarryInventory inventory, IReactiveCommand<DiedArgs> healthDeath)
		{
			if(_currentContext)
				_currentContext.Inventory.UnSelect();
			
			_inventory = inventory;
			_disposable?.Dispose();
			_disposable = healthDeath.Subscribe(OnContextDied);
			_playerInputController.ActiveInputRig.Value.OnSelectedItemChanged(_inventory.CurrentCarried.Value);
			_playerInputController.SwitchInputRig(InputRigType.CarryingInputRig);
		}

		public void Detach()
		{
			_disposable?.Dispose();
			_inventory = null;
			DoSome2().Forget();
		}

		private async UniTaskVoid DoSome2()
		{
			await UniTask.DelayFrame(3);
			_playerInputController.SwitchInputRig(InputRigType.PlayerInputRig);
		}
		
		private void OnContextDied(DiedArgs args)
		{
			Detach();
		}

		public void Dispose()
		{
			_playerInputController?.Dispose();
			_disposable?.Dispose();
			_subscription?.Dispose();
		}
	}

	public interface ICarryInventoryService
	{
		public void Attach(CarryInventory inventory, IReactiveCommand<DiedArgs> healthDeath);

		public void Detach();
		public void DetachCurrent();
	}
	
}
