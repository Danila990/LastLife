using System;
using Cinemachine;
using Core.Entity;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.CameraSystem
{
	public abstract class CameraController : MonoBehaviour, AxisState.IInputAxisProvider, ICameraController
	{
		[SerializeField] private Transform _camTarget;
		[SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
		protected AudioListener AudioListener;
		public abstract CameraType CameraType { get; }
		public CinemachinePOV CinemachinePov { get; private set; }
		public Transform CameraTarget => _camTarget;
		public CinemachineVirtualCamera CinemachineVCam => _cinemachineVirtualCamera;
		private Vector3 _lastPos;
		private MotionHandle _fovTween;
		protected float Sensitivity = 1;
		
		
		[ShowInInspector] private ICameraTargetEntity _currentEntityTarget;
		private SimpleCameraInputProxy _inputProxy;
		public ICameraTargetEntity CameraTargetEntity => _currentEntityTarget;

		public void Init(SimpleCameraInputProxy inputProxy)
		{
			_inputProxy = inputProxy;
			
			CinemachinePov = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
			if (CinemachinePov)
			{
				CinemachinePov.m_HorizontalAxis.SetInputAxisProvider(0, this);
				CinemachinePov.m_VerticalAxis.SetInputAxisProvider(1, this);
			}
			InitInternal();
		}
		
		protected virtual void InitInternal() {}

		public void SetAudioListener(AudioListener audioListener)
		{
			AudioListener = audioListener;
		}

		public void SetFov(float amount, float duration)
		{
			_fovTween.IsActiveCancel();
			_fovTween = LMotion.Create(CinemachineVCam.m_Lens.FieldOfView, amount, duration)
				.Bind(value => CinemachineVCam.m_Lens.FieldOfView = value);
		}
		
		public virtual float GetAxisValue(int axis)
		{
			var res =  axis switch
			{
				0 => _inputProxy.GetAxisValue(axis) * Sensitivity,
				1 => _inputProxy.GetAxisValue(axis) * Sensitivity,
				2 => _inputProxy.GetAxisValue(axis),
				_ => throw new ArgumentException($"Axis exception {axis}")
			};
			return res;
		}
        
		public void UpdateFollowPos(bool force = false)
		{
			if (_currentEntityTarget == null) return;
			if (_currentEntityTarget.TargetIsActive || force)
			{
				if(_currentEntityTarget.CameraTargetRoot)
					_lastPos = _currentEntityTarget.CameraTargetRoot.position;
			}
			_camTarget.position = _lastPos;
			if (_currentEntityTarget.CameraTargetRoot)
			{
				UpdateAudioListener();
			}
		}

		protected virtual void UpdateAudioListener()
		{
		}
        
		public void SetFollowTarget(ICameraTargetEntity target)
		{
			_currentEntityTarget = target;
			OnSetFollowTarget();
		}
		
		protected abstract void OnSetFollowTarget();

		public void SetVCamActive(bool state)
		{ 
			if (state)
			{
				CinemachineVCam.Priority = 10;
			}
			else
			{
				CinemachineVCam.Priority = -1;
			}
			OnSetActive(state);	
		}

		protected virtual void OnSetActive(bool state) { }
		
		private void OnDestroy()
		{
			_fovTween.IsActiveCancel();
		}
	}
}