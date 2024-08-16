using System;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using LitMotion;
using UniRx;
using UnityEngine;

namespace Core.CameraSystem
{
	public class FirstPersonCameraController : CameraController
	{
		public override CameraType CameraType => CameraType.Fpv;

		public FirstPersonHands FPVHands;
		public Animation ItemAnimation;
		private IDisposable _disposable;
		private MotionHandle _motion;
		private float _fieldOfView;
		
		protected override void InitInternal()
		{
			_fieldOfView = CinemachineVCam.m_Lens.FieldOfView;
		}

		protected override void OnSetFollowTarget()
		{
			_disposable?.Dispose();
            
			if (CameraTargetEntity is CharacterContext characterContext)
			{
				_disposable = characterContext.CurrentAdapter.AimController.TargetAim.Subscribe(OnAim);
			}
		}

		protected override void UpdateAudioListener()
		{
			var listPos = CameraTargetEntity.CameraTargetRoot.position - transform.forward * (2.5f);
			AudioListener.transform.position = listPos;
		}
		
		protected override void OnSetActive(bool state)
		{
			if (!state)
			{
				FPVHands.gameObject.SetActive(false);
			}
			if (CameraTargetEntity is CharacterContext characterContext)
			{
				foreach (var render in characterContext.BodyRenders)
				{
					render.enabled = !state;
				}
			}	
		}
		
		private void OnAim(AimStatus state)
		{
			if (_motion.IsActive())
			{
				_motion.Cancel();
			}
			
			if (state.State!=AimState.Default && state.State==AimState.Sniper)
			{
				_motion = LMotion
					.Create(0, 1f, 0.5f)
					.Bind(OnSniperEnable);

			}
			else
			{
				_motion = LMotion
					.Create(0, 1f, 0.5f)
					.Bind(OnAimDisable);
			}
		}
		
		private void OnSniperEnable(float lerp)
		{
			CinemachineVCam.m_Lens.FieldOfView = Mathf.Lerp(CinemachineVCam.m_Lens.FieldOfView, 10, lerp);
			Sensitivity = 0.25f;
		}

		private void OnAimDisable(float lerp)
		{
			CinemachineVCam.m_Lens.FieldOfView = Mathf.Lerp(CinemachineVCam.m_Lens.FieldOfView, _fieldOfView, lerp);
			Sensitivity = 1f;
		}
	}
}