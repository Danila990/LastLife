#if RELEASE_BRANCH
using RemoteConfig.Impl.GameAnalyticRemoteConfig;
#else
using RemoteConfig.Impl;
#endif
using VContainer;
using VContainer.Extensions;

namespace RemoteConfig.Installer
{
	public class RemoteConfigsInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
#if RELEASE_BRANCH
			builder.Register<GameAnalyticRemoteConfig>(Lifetime.Singleton).AsImplementedInterfaces();
#else
			builder.Register<MockRemoteConfigAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
#endif
		}
	}
}