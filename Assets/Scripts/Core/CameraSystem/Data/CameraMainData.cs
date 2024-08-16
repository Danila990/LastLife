using Cinemachine;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Extensions;
using VContainer.Unity;

namespace Core.CameraSystem.Data
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(CameraMainData), fileName = nameof(CameraMainData), order = 0)]
	public class CameraMainData : ScriptableObjectInstaller
	{
		[field:SerializeField] public ThirdPersonCameraController ThirdPersonPrefab { get; set; }
		[field:SerializeField] public FirstPersonCameraController FirstPersonPrefab { get; set; }
		[field:SerializeField] public CutsceneCamera CutsceneCamera { get; set; }
		[field:SerializeField] public CinemachineBrain Brain { get; set; }
		
		public override void Install(IContainerBuilder builder)
		{
			var cameraHolder = new GameObject("--- Camera Holder ---").transform;
			
			builder.RegisterComponentInNewPrefab(Brain, Lifetime.Singleton).UnderTransform(cameraHolder);

			builder.Register(resolver =>
			{
				var tpvObj = Instantiate(ThirdPersonPrefab, cameraHolder);
				//resolver.Inject(tpvObj);
				return tpvObj;
			}, Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
			
			builder.Register(resolver =>
			{
				var tpvObj = Instantiate(FirstPersonPrefab, cameraHolder);
				//resolver.Inject(tpvObj);
				return tpvObj;
			}, Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
			
			builder.Register(resolver =>
			{
				var tpvObj = Instantiate(CutsceneCamera, cameraHolder);
				//resolver.Inject(tpvObj);
				return tpvObj;
			}, Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
			
			//builder.RegisterComponentInNewPrefab(ThirdPersonPrefab, Lifetime.Singleton).UnderTransform(cameraHolder).As<ICameraController>();
			//builder.RegisterComponentInNewPrefab(FirstPersonPrefab, Lifetime.Singleton).UnderTransform(cameraHolder).As<ICameraController>();
		}
	}
}