using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using Core.Services.Input;
using Sirenix.OdinInspector;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.CameraSystem
{
	public class CameraService : ICameraService, IInitializable, ITickable, IPostLateTickable, IDisposable
	{
		private readonly IInputService _inputService;
		private readonly ReactiveProperty<bool> _isThirdPerson;
		private readonly ReactiveProperty<CameraType> _cameraTypeChanged;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly Dictionary<CameraType, ICameraController> _cameraControllers;
		private readonly CinemachineBrain _brain;
		private readonly IObjectResolver _resolver;

		private AudioListener _audioListener;
		private CharacterContext _currentContext;
		private SimpleCameraInputProxy _inputProxy;

		[ShowInInspector] public ICameraController CurrentCameraController { get; private set; }
		[ShowInInspector] public FirstPersonCameraController FpvCam { get; private set; }
		[ShowInInspector] public ThirdPersonCameraController TpvCam { get; private set; }
		
		[ShowInInspector] public CinemachineBrain CurrentBrain => _brain;
		[ShowInInspector] public bool IsThirdPerson => _isThirdPerson.Value;
		public IObservable<bool> IsThirdPersonObservable => _isThirdPerson;
		public IObservable<CameraType> OnCameraTypeChanged => _cameraTypeChanged;

		public bool CanSwitchCamera { get; set; }

		public CameraService(
			IInputService inputService,
			CinemachineBrain brain, 
			IEnumerable<ICameraController> cameraControllers,
			IObjectResolver resolver
				)
		{
			_inputService = inputService;
			_brain = brain;
			_resolver = resolver;
			_cameraControllers = cameraControllers.ToDictionary(controller => controller.CameraType);
			_isThirdPerson = new BoolReactiveProperty().AddTo(_compositeDisposable);
			_cameraTypeChanged = new ReactiveProperty<CameraType>().AddTo(_compositeDisposable);
			CanSwitchCamera = true;
		}

		public void Initialize()
		{
			_isThirdPerson.Value = true;
			_cameraTypeChanged.Value = CameraType.Tpv;
			InitializeCameras();
			SetThirdPerson();

			_inputService
				.ObserveGetButtonDown("Camera_Switch")
				.Subscribe(SwitchCamera)
				.AddTo(_compositeDisposable);
		}
		
		private void OnContextChanged(CharacterContext context)
		{
			if(!context) 
				return;
			_currentContext = context;
			//context.Health.OnDeath.Subscribe(ContextSetThirdPerson).AddTo(context);
			SetTrackedTarget(context);
		}
		
		private void ContextSetThirdPerson(DiedArgs _)
		{
			SetThirdPerson();
		}

		public void SetTrackedAdapter(BaseCharacterAdapter playerAdapter)
		{
			playerAdapter.ContextChanged.Subscribe(OnContextChanged).AddTo(_compositeDisposable);
			SetThirdPerson();
		}

		private void InitializeCameras()
		{
			_audioListener = _brain.GetComponentInChildren<AudioListener>();
			_inputProxy = new SimpleCameraInputProxy(_inputService, _resolver.Resolve<PlayerInputController>());
		
			if (_cameraControllers.TryGetValue(CameraType.Fpv, out var cam))
			{
				FpvCam = (FirstPersonCameraController)cam;
			}
			
			if (_cameraControllers.TryGetValue(CameraType.Tpv, out var tpv))
			{
				TpvCam = (ThirdPersonCameraController)tpv;
			}
			
			foreach (var cameraController in _cameraControllers.Values)
			{
				cameraController.Init(_inputProxy);
				cameraController.SetAudioListener(_audioListener);
				cameraController.SetVCamActive(false);
			}
		}

		public void SetThirdPerson()
		{
			SetCameraController(CameraType.Tpv);
		}

		public void SetFirstPerson()
		{
			if (!_currentContext)
					return;
			SetCameraController(CameraType.Fpv);
		}
		
		public void SetCutsceneCamera()
		{
			SetCameraController(CameraType.Cutscene);
		}
		
		public void SetCameraByType(CameraType lastCameraType)
		{
			switch (lastCameraType)
			{
				case CameraType.Fpv:
					SetFirstPerson();
					return;
				case CameraType.Tpv:
					SetThirdPerson();
					return;
				case CameraType.Cutscene:
					SetCutsceneCamera();
					return;
			}
		}

		private void SetCameraController(CameraType cameraType)
		{
			Vector2 axis = default;
			if (CurrentCameraController != null && CurrentCameraController.CinemachinePov)
			{
				CurrentCameraController.SetVCamActive(false);
				axis.x = CurrentCameraController.CinemachinePov.m_HorizontalAxis.Value;
				axis.y = CurrentCameraController.CinemachinePov.m_VerticalAxis.Value;
			}
			
			CurrentCameraController = _cameraControllers[cameraType];
			CurrentCameraController.UpdateFollowPos(true);

			if (CurrentCameraController.CinemachinePov && axis != default(Vector2))
			{
				CurrentCameraController.CinemachinePov.m_HorizontalAxis.Value = axis.x;
				CurrentCameraController.CinemachinePov.m_VerticalAxis.Value = axis.y;
			}

			CurrentCameraController.SetVCamActive(true);
			_cameraTypeChanged.Value = cameraType;
			_isThirdPerson.Value = cameraType == CameraType.Tpv;
		}

		public void SetTrackedTarget(ICameraTargetEntity cameraTargetEntity)
		{
			TpvCam?.SetFollowTarget(cameraTargetEntity);
			FpvCam?.SetFollowTarget(cameraTargetEntity);
		}

		public void SetZoomStatus(bool status)
		{
			const float duration = 0.25f;
			if (status)
			{
				FpvCam?.SetFov(15, duration);
				TpvCam?.SetFov(15, duration);
				return;
			}
			FpvCam?.SetFov(60, duration);
			TpvCam?.SetFov(50, duration);
		}

		public void SwitchCamera(Unit _)
		{
			if (!CanSwitchCamera)
				return;
			var stat = !_isThirdPerson.Value;
			if (stat)
			{
				SetThirdPerson();
			}
			else
			{
				SetFirstPerson();
			}
			_brain.ManualUpdate();
		}

		public void Dispose()
		{
			_isThirdPerson?.Dispose();
			_compositeDisposable?.Dispose();
		}
		
		public void Tick()
		{
			CurrentCameraController.UpdateFollowPos();
			_brain.ManualUpdate();
		}
		
		public void PostLateTick()
		{
			CurrentCameraController.UpdateFollowPos();
			_brain.ManualUpdate();
		}
	}

	public interface ICameraService
	{
		ICameraController CurrentCameraController { get; }
		CinemachineBrain CurrentBrain { get; }
		IObservable<bool> IsThirdPersonObservable { get; }
		IObservable<CameraType> OnCameraTypeChanged { get; }
		FirstPersonCameraController FpvCam { get; }
		ThirdPersonCameraController TpvCam { get; }
		bool IsThirdPerson { get; }
		bool CanSwitchCamera { get; set; }
		void SetTrackedAdapter(BaseCharacterAdapter playerAdapter);
		void SetThirdPerson();
		void SetFirstPerson();
		void SetTrackedTarget(ICameraTargetEntity cameraTargetEntity);
		void SetZoomStatus(bool status);
		void SetCutsceneCamera();
		void SetCameraByType(CameraType lastCameraType);
	}
}