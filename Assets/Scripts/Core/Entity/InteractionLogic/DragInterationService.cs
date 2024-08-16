using System;
using Core.CameraSystem;
using Core.InputSystem;
using Core.Services;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Entity.InteractionLogic
{
	public interface IDragInteractionService
	{
		void OnInput(bool stat);
		public DragInteractor DragInteractor { get; }
		public bool TryInteract { get; }
		public IReactiveProperty<bool> InputStatus { get; }
	}

	public class DragInteractionService :
		IDragInteractionService,
		IPostInitializable,
		IFixedTickable,
		IPostLateTickable, 
		IDisposable,
		ITickable
	{
		private readonly ICameraService _camera;
		private readonly IRayCastService _rayCastService;
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IInputService _inputService;
		private readonly PlayerInputView _playerInputView;
		private readonly CompositeDisposable _compositeDisposable = new();
		private readonly ReactiveProperty<bool> _inputStatus = new();
		private bool _tryInteract;
		private float _delta;


		private Transform _cameraTransform;
		private DragInteractor _interactor;
		public DragInteractor DragInteractor => _interactor;
		public bool TryInteract => _tryInteract;
		public IReactiveProperty<bool> InputStatus => _inputStatus;

		public DragInteractionService(
			ICameraService camera,
			IRayCastService rayCastService,
			IPlayerSpawnService playerSpawnService,
			IInputService inputService,
			PlayerInputView playerInputView
		)
		{
			_camera = camera;
			_rayCastService = rayCastService;
			_playerSpawnService = playerSpawnService;
			_inputService = inputService;
			_playerInputView = playerInputView;
		}

		public void Tick()
		{
			_interactor.SetCurrDist(Mathf.Clamp(_interactor.CurrentDist + _delta * 10 * Time.deltaTime, _rayCastService.PlaneDist, 100));
		}

		public void FixedTick()
		{
			if (_interactor == null)
				return;
					
			_interactor.FixedUpdate(Time.fixedDeltaTime);
			if (!_tryInteract) 
				return;
			
			Interact();
			
			if (!_interactor.InDrag) 
				return;
			_tryInteract = false;
		}

		public void PostLateTick()
		{
			_interactor.Update();
		}

		public void PostInitialize()
		{
			_cameraTransform = _camera.CurrentBrain.transform;
			_interactor = new(_cameraTransform, _rayCastService);
			_inputService.ObserveGetAxis("DistanceChange").Subscribe(OnDistChange).AddTo(_compositeDisposable);
			_interactor.DragStatus.Subscribe(OnDragChange).AddTo(_compositeDisposable);
		}

		private void OnDragChange(bool status)
		{
			_playerInputView.DistanceChange.SetActive(status);
		}

		private void OnDistChange(float delta)
		{
			_delta = delta;
		}

		public void OnInput(bool stat)
		{
			_inputStatus.Value = stat;
			if (_interactor.InDrag || !stat)
			{
				_interactor.Release();
				_tryInteract = false;
				return;
			}
			Interact();
			_tryInteract = !_interactor.InDrag;
			//Interact();
		}

		private void Interact()
		{
			_rayCastService.SphereCastRayInteract(0.15f, true, _interactor, 100, _playerSpawnService.PlayerCharacterAdapter.CurrentContext.Uid);
		}

		public void Dispose()
		{
			_interactor?.Dispose();
			_compositeDisposable?.Dispose();
			_inputStatus?.Dispose();
		}
	}
}