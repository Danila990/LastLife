using Cinemachine;
using Core.Entity;
using UnityEngine;

namespace Core.CameraSystem
{
	public interface ICameraController
	{
		CameraType CameraType { get; }
		CinemachinePOV CinemachinePov { get; }
		Transform CameraTarget { get; }
		CinemachineVirtualCamera CinemachineVCam { get; }
		ICameraTargetEntity CameraTargetEntity { get; }
		void Init(SimpleCameraInputProxy inputService);
		void SetAudioListener(AudioListener audioListener);
		void SetFov(float amount, float duration);
		void UpdateFollowPos(bool force = false);
		void SetFollowTarget(ICameraTargetEntity target);
		void SetVCamActive(bool state);
	}
}