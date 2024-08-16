using System;
using Cinemachine;
using Core.Services.Input;
using Ui.Sandbox.PlayerInput;
using Utils.Constants;

namespace Core.CameraSystem
{
	public class SimpleCameraInputProxy : AxisState.IInputAxisProvider
	{
		private readonly IInputService _inputService;
		private readonly PlayerInputController _playerInputController;
		
		public SimpleCameraInputProxy(IInputService inputService, PlayerInputController playerInputController)
		{
			_inputService = inputService;
			_playerInputController = playerInputController;
		}
		
		public float GetAxisValue(int axis)
		{
			if (_playerInputController.ActiveInputRig.Value && !_playerInputController.ActiveInputRig.Value.RigIsActive)
				return 0;

			var res =  axis switch
			{
				0 => _inputService.GetAxis(InputConsts.MOUSE_X),
				1 => _inputService.GetAxis(InputConsts.MOUSE_Y),
				2 => _inputService.GetAxis("Scroll Wheel"),
				_ => throw new ArgumentException($"Axis exception {axis}")
			};
			return res;
		}
	}
}