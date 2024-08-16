using Banner;
using Core.Boosts.Inventory;
using Core.Factory.Inventory;
using Core.Factory.Ui;
using Core.Map;
using Core.Services;
using Core.Services.Experience;
using Core.Services.SaveSystem.SaveAdapters;
using Core.Services.SaveSystem.SaveAdapters.EntitySave;
using Market.Bank;
using Market.OilRig;
using Sandbox.Initializers;
using VContainer;
using VContainer.Extensions;

namespace Installer.Market
{

	public class MarketInstaller : MonoInstaller
	{
		public ExperienceData ExperienceData;

		public override void Install(IContainerBuilder builder)
		{
			RegisterFactories(builder);
			RegisterServices(builder);
			RegisterOthers(builder);

			builder.RegisterInstance(ExperienceData);
		}
		
		private void RegisterOthers(IContainerBuilder builder)
		{
			builder.Register<MarketInitializer>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BannerService>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<MarketPointProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BankService>(Lifetime.Singleton).AsImplementedInterfaces();
			
			//Saves
			builder.Register<EntitySaveRepositoryAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RefinerSaveAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ConveyorProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ConveyorSaveAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<InterstitialByTimer>(Lifetime.Singleton).AsImplementedInterfaces(); 
			builder.Register<TrackSpendSceneTimeService>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		private void RegisterServices(IContainerBuilder builder)
		{
			builder.Register<RouletteService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerTeleportProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RefineProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RefinerBoostService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BoostInventoryRuntimeSaver>(Lifetime.Singleton).AsImplementedInterfaces();
			
			//builder.Register<PathfindingService>(Lifetime.Singleton).AsImplementedInterfaces();
			//builder.Register<CharacterSelectInterstitial>(Lifetime.Singleton).AsImplementedInterfaces();
			//builder.Register<PlayerWorldInteractorService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
		
		private void RegisterFactories(IContainerBuilder builder)
		{

			builder.Register<SpawnMenuItemFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<InventoryItemsFactory>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}