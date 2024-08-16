using Core.Actions;
using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.InputSystem;
using UniRx;
using UnityEngine;
using VContainer;

namespace Ui.Sandbox.PlayerInput
{
	public class AimInputActionButton : AdditionalInputActionButton
	{
		[SerializeField] private ItemEntityActionController _action;

		[Inject] private readonly ICameraService _cameraService;
		[Inject] private readonly IPlayerSpawnService _playerSpawnService;
		private PlayerCharacterAdapter _playerCharacterAdapter;

		public override void Init()
		{
			SetUp(_action);
			base.Init();
			_playerCharacterAdapter = _playerSpawnService.PlayerCharacterAdapter;
			_playerCharacterAdapter.AimController.TargetAim.Subscribe(OnAim).AddTo(this);
			_cameraService.IsThirdPersonObservable.Subscribe(OnChangeView).AddTo(this);
		}
		
		private void OnChangeView(bool status)
		{
			if (status)
			{
				OnAim(_playerCharacterAdapter.AimController.TargetAim.Value);
			}
			else
			{
				Hide();
			}
		}

		private void OnAim(AimStatus status)
		{
			if (!_cameraService.IsThirdPerson)
				return;
			
			if (status.State != AimState.Default)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		public override void OnContextChanged(CharacterContext characterContext)
		{
			
		}
	}

	public abstract class AdditionalInputActionButton : FreakInputActionButton
	{
	}
}