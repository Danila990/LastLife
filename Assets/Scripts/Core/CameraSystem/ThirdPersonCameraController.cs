using System;
using Cinemachine;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using LitMotion;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.CameraSystem
{
	public class ThirdPersonCameraController : CameraController
	{
		public override CameraType CameraType => CameraType.Tpv;

		private CharacterContext _characterContext;
		private IDisposable _disposable;
		private CinemachineFramingTransposer _framingTransposer;
		private float _baseCameraDistance;
		private Vector2 _damping;
		private float _fieldOfView;
		private MotionHandle _motion;
		private AimState _currState;

		protected override void InitInternal()
		{
			_framingTransposer = CinemachineVCam.GetCinemachineComponent<CinemachineFramingTransposer>();
			_baseCameraDistance = _framingTransposer.m_CameraDistance;
			_fieldOfView = CinemachineVCam.m_Lens.FieldOfView;
			_damping.x = _framingTransposer.m_XDamping;
			_damping.y = _framingTransposer.m_ZDamping;
		}

		protected override void OnSetFollowTarget()
		{
			_disposable?.Dispose();

			if (CameraTargetEntity is CharacterContext characterContext)
			{
				_disposable = characterContext.CurrentAdapter.AimController.TargetAim.Subscribe(OnAim);
			}
			_framingTransposer.m_CameraDistance = _baseCameraDistance + CameraTargetEntity.AdditionalCameraDistance;
		}

		protected override void UpdateAudioListener()
		{
			var listPos = CameraTargetEntity.CameraTargetRoot.position - transform.forward * (2.5f);
			AudioListener.transform.position = listPos;
		}

		private void OnDisable()
		{
			_disposable?.Dispose();
			_motion.IsActiveCancel();
		}

		private void OnAim(AimStatus state)
		{
			if (state.State != AimState.Default)
			{
				//_framingTransposer.m_CameraDistance = _baseCameraDistance - _aimCameraAdditionalDistance;
				_motion.IsActiveCancel();
				_motion = LMotion
					.Create(0, 1f, 0.5f)
					.Bind(state.State == AimState.Sniper ? OnSniperEnable : OnAimEnable);

			}
			else
			{
				_motion.IsActiveCancel();
				_motion = LMotion
					.Create(0, 1f, 0.5f)
					.Bind(OnAimDisable);
			}
		}

		private void OnSniperEnable(float lerp)
		{
			_framingTransposer.m_XDamping = Mathf.Lerp(_framingTransposer.m_XDamping, 0, lerp);
			_framingTransposer.m_ZDamping = Mathf.Lerp(_framingTransposer.m_ZDamping, 0, lerp);
			CinemachineVCam.m_Lens.FieldOfView = Mathf.Lerp(CinemachineVCam.m_Lens.FieldOfView, 10, lerp);
			Sensitivity = 0.25f;
		}

		private void OnAimEnable(float lerp)
		{
			_framingTransposer.m_XDamping = Mathf.Lerp(_framingTransposer.m_XDamping, 0, lerp);
			_framingTransposer.m_ZDamping = Mathf.Lerp(_framingTransposer.m_ZDamping, 0, lerp);
			CinemachineVCam.m_Lens.FieldOfView = Mathf.Lerp(CinemachineVCam.m_Lens.FieldOfView, 40, lerp);
			Sensitivity = 1f;
		}

		private void OnAimDisable(float lerp)
		{
			_framingTransposer.m_XDamping = Mathf.Lerp(_framingTransposer.m_XDamping, _damping.x, lerp);
			_framingTransposer.m_ZDamping = Mathf.Lerp(_framingTransposer.m_ZDamping, _damping.y, lerp);
			CinemachineVCam.m_Lens.FieldOfView = Mathf.Lerp(CinemachineVCam.m_Lens.FieldOfView, _fieldOfView, lerp);
			Sensitivity = 1f;
		}
	}
}