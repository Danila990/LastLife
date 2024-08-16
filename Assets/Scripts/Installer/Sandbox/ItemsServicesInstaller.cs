using Core.Services;
using Ui.Sandbox.SpawnMenu;
using VContainer;
using VContainer.Extensions;

namespace Installer.Sandbox
{
	public class ItemsServicesInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<ScriptableActionsInitializer>(Lifetime.Singleton).AsImplementedInterfaces();

			builder.Register<CircularCharacterDestroyingService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SwitchItemService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SpawnItemService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<DeleteItemService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}