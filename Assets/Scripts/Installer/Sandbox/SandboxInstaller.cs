using Banner;
using Core.Boosts.Inventory;
using Core.Factory;
using Core.Factory.Inventory;
using Core.Factory.Ui;
using Core.Map;
using Core.Services;
using Core.Services.Experience;
using InAppReview;
using Sandbox.Initializers;
using VContainer;
using VContainer.Extensions;
using VContainer.Unity;

namespace Installer.Sandbox
{
	public class SandboxInstaller : MonoInstaller
	{
		public ExperienceData ExperienceData;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<IAPReview>();

			RegisterFactories(builder);
			RegisterServices(builder);
			RegisterOthers(builder);
			
			builder.RegisterInstance(ExperienceData);
		}
		
		private void RegisterOthers(IContainerBuilder builder)
		{
			builder.Register<SandboxInitializer>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BannerService>(Lifetime.Singleton).AsImplementedInterfaces();

			
			builder.Register<TrackSpendSceneTimeService>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		private void RegisterServices(IContainerBuilder builder)
		{
			//BOSS
			builder.Register<BossProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BossSpawnService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SpawnPointWithBossProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			//
			builder.Register<RouletteService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PathfindingService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerTeleportWithBossProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BoostInventoryRuntimeSaver>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<MechAdapterFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<MechSpawnFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			//
			builder.Register<InterstitialByTimer>(Lifetime.Singleton).AsImplementedInterfaces();
		}
		
		private void RegisterFactories(IContainerBuilder builder)
		{
			builder.Register<AiHeadFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SpawnMenuItemFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<InventoryItemsFactory>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}