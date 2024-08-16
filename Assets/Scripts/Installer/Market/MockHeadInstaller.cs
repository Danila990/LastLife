using Core.Factory;
using Core.Services;
using VContainer;
using VContainer.Extensions;

namespace Installer.Market
{
	public class MockHeadInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<AiHeadFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<TestPathfindingService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}