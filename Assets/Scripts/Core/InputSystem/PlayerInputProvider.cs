using System;
using Core.Actions;
using Core.Services.Input;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer.Unity;

namespace Core.InputSystem
{
	public class PlayerInputProvider : IInitializable, IDisposable, IPlayerInputProvider
	{
		private readonly IInputService _inputService;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

		private IPlayerInputListener _inputListener;
		private bool _moveStarted = false;

		public PlayerInputProvider(
			IInputService inputService
			)
		{
			_inputService = inputService;
		}
		
		public void UseListener(IPlayerInputListener listener)
		{
			if (_inputListener is not null)
			{
				_inputListener.InputDto.Move = Vector2.zero;
				_inputListener.InputDto.MoveRaw = Vector2.zero;
			}
			_inputListener = listener;
		}
		
		public void Initialize()
		{
			_inputService.ObserveGetAxis2D(InputConsts.HORIZONTAL, InputConsts.VERTICAL).Subscribe(MoveInput).AddTo(_compositeDisposable);
			_inputService.ObserveGetAxis2D(InputConsts.MOUSE_X, InputConsts.MOUSE_Y).Subscribe(LookInput).AddTo(_compositeDisposable);
			
			_inputService.ObserveGetAxis2DRaw(InputConsts.HORIZONTAL, InputConsts.VERTICAL).Subscribe(MoveInputRaw).AddTo(_compositeDisposable);
			
			_inputService.ObserveGetButton(InputConsts.SPRINT).Subscribe(SprintInput).AddTo(_compositeDisposable);

			_inputService.ObserveGetButtonDown(InputConsts.JUMP).Subscribe(JumpInputDown).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonUp(InputConsts.JUMP).Subscribe(JumpInputUp).AddTo(_compositeDisposable);
			//_inputService.ObserveGetButtonDown(InputConsts.SPRINT).Subscribe(SprintDown).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonDown(InputConsts.AIM).Subscribe(OnAimDown).AddTo(_compositeDisposable);
			
			_inputService.ObserveGetButtonDown(InputConsts.MAIN_ACTION).SubscribeWithState2(ActionKey.MainAction, InputKeyType.GetDown, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonDown(InputConsts.ACTION_ONE).SubscribeWithState2(ActionKey.ActionOne, InputKeyType.GetDown, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonDown(InputConsts.ACTION_TWO).SubscribeWithState2(ActionKey.ActionTwo, InputKeyType.GetDown, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonDown(InputConsts.ACTION_THIRD).SubscribeWithState2(ActionKey.ActionThird, InputKeyType.GetDown, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonDown(InputConsts.ACTION_FOUR).SubscribeWithState2(ActionKey.ActionFour, InputKeyType.GetDown, OnAction).AddTo(_compositeDisposable);
			
			
			_inputService.ObserveGetButtonUp(InputConsts.MAIN_ACTION).SubscribeWithState2(ActionKey.MainAction, InputKeyType.GetUp, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonUp(InputConsts.ACTION_ONE).SubscribeWithState2(ActionKey.ActionOne, InputKeyType.GetUp, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonUp(InputConsts.ACTION_TWO).SubscribeWithState2(ActionKey.ActionTwo, InputKeyType.GetUp, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonUp(InputConsts.ACTION_THIRD).SubscribeWithState2(ActionKey.ActionThird, InputKeyType.GetUp, OnAction).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonUp(InputConsts.ACTION_FOUR).SubscribeWithState2(ActionKey.ActionFour, InputKeyType.GetUp, OnAction).AddTo(_compositeDisposable);


			_inputService.ObserveGetButton(InputConsts.MAIN_ACTION).SubscribeWithState(ActionKey.MainAction, OnActionGet).AddTo(_compositeDisposable);
			_inputService.ObserveGetButton(InputConsts.ACTION_ONE).SubscribeWithState(ActionKey.ActionOne, OnActionGet).AddTo(_compositeDisposable);
			_inputService.ObserveGetButton(InputConsts.ACTION_TWO).SubscribeWithState(ActionKey.ActionTwo, OnActionGet).AddTo(_compositeDisposable);
			_inputService.ObserveGetButton(InputConsts.ACTION_THIRD).SubscribeWithState(ActionKey.ActionThird, OnActionGet).AddTo(_compositeDisposable);
			_inputService.ObserveGetButton(InputConsts.ACTION_FOUR).SubscribeWithState(ActionKey.ActionFour, OnActionGet).AddTo(_compositeDisposable);
		}
		
		private void OnActionGet(bool arg1, ActionKey arg2)
		{
			_inputListener?.OnActionGet(arg2, arg1);
		}
		
		private void OnAction(Unit _, ActionKey actionKey, InputKeyType inputKeyType)
		{
			_inputListener?.OnAction(actionKey, inputKeyType);
		}

		private void OnAimDown(Unit obj)
		{
			_inputListener?.OnAimDown();
		}

		private void MoveInputRaw(Vector2 value)
		{
			if (_inputListener != null)
			{
				_inputListener.InputDto.MoveRaw = value;
			}
			if (value != Vector2.zero)
			{
				if (!_moveStarted)
					return;
				_moveStarted = false;
				_inputListener?.OnMoveDown();
			}
			else
			{
				_moveStarted = true;
			}
		}

		private void SprintDown(Unit obj)
		{
			_inputListener?.OnSprintDown();  
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			if (_inputListener != null)
			{
				_inputListener.InputDto.Move = newMoveDirection;
			}
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			if (_inputListener != null)
			{
				_inputListener.InputDto.Look = newLookDirection;
			}
		}

		public void JumpInputDown(Unit newJumpState)
		{
			_inputListener?.OnJumpDown();
		}

		public void JumpInputUp(Unit newJumpState)
		{
			_inputListener?.OnJumpUp();
		}
		
		public void SprintInput(bool newJumpState)
		{
			if (_inputListener != null)
			{
				_inputListener.InputDto.Sprint = newJumpState;
			}
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
		
	}
}