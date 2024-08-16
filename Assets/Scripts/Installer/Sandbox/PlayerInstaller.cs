using Core.Factory;
using Core.InputSystem;
using Core.Services;
using Core.Services.Experience;
using VContainer;
using VContainer.Extensions;

namespace Installer.Sandbox
{
	public class PlayerInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			RegisterServices(builder);
		}

		private void RegisterServices(IContainerBuilder builder)
		{
			builder.Register<PlayerStaticInteractionService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerInputProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerSpawnService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerCharacterFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			//Exp
			builder.Register<ExperienceService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ExperienceEntityDeathListener>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}