using Core.Services;
using UnityEngine;
using VContainer;
using VContainer.Extensions;

namespace Installer.Launch
{
	public class LaunchInstaller : MonoInstaller
	{
		[SerializeField] private bool _autoLaunchNextScene = true;

		public override void Install(IContainerBuilder builder)
		{
			builder.Register<LaunchInitialize>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter(_autoLaunchNextScene);
			builder.Register<TrackSpendSceneTimeService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}