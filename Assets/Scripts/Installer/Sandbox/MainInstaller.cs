using Core.Actions.SpecialAbilities;
using Core.Boosts;
using Core.Boosts.Inventory;
using Core.CameraSystem;
using Core.Carry;
using Core.Entity.InteractionLogic;
using Core.Entity.Repository;
using Core.Equipment;
using Core.Factory;
using Core.Factory.VFXFactory.Impl;
using Core.FullScreenRenderer;
using Core.Inventory;
using Core.Loot;
using Core.PlayerDeath;
using Core.Projectile.Projectile;
using Core.Render;
using Core.Services;
using Core.Timer;
using Core.Zones;
using LostConnection;
using Shop.Impl;
using Ui.Sandbox.WorldSpaceUI;
using Utils;
using VContainer;
using VContainer.Extensions;

namespace Installer.Sandbox
{
	public class MainInstaller : MonoInstaller
	{
		private InstallerCancellationToken _installerCancellation;
		
		public override void Install(IContainerBuilder builder)
		{
			_installerCancellation = new InstallerCancellationToken(destroyCancellationToken);
			builder.RegisterInstance(_installerCancellation);
			
			RegisterFactories(builder);
			RegisterServices(builder);
			RegisterInteractions(builder);
		}
		
		private void RegisterInteractions(IContainerBuilder builder)
		{
			builder.Register<RayCastInteractionService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<DragInteractionService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<OverlapInteractionService>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		private void RegisterServices(IContainerBuilder builder)
		{
			// Core
			builder.Register<EntityRepository>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<CameraService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ProjectileService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<MapService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<MapZoneService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<CarryInventoryService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerEventPublisher>(Lifetime.Singleton).AsImplementedInterfaces();

			// Loot
			builder.Register<LootService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<LootableEntityDeathListener>(Lifetime.Singleton).AsImplementedInterfaces();
			// Ui
			builder.Register<MenuPanelService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<WorldSpaceUIService>(Lifetime.Singleton).AsImplementedInterfaces();
			
			// Boost
			builder.Register<PlayerPassRendererProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			
			// Inventories
			builder.Register<InventoryService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<InventorySaveAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<EquipmentInventoryService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<EquipmentRecreateService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<EquipmentShopHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<EquipmentSaveAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BoostInventorySaveAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BoostInventoryService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<AbilitiesControllerService>(Lifetime.Singleton).AsImplementedInterfaces();

			
			// Zones
			builder.Register<ZoneInteractionService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SandboxZonesDescriber>(Lifetime.Singleton).AsImplementedInterfaces();
			
			// Render
			builder.Register<RendererStorage>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PassRendererProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			
			//Shop
			builder.Register<BoostPurchaseService>(Lifetime.Singleton).AsImplementedInterfaces();

			builder.Register<TimerService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<TimerProvider>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		private static void RegisterFactories(IContainerBuilder builder)
		{
			builder.Register<LootFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<AdapterStrategyFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ObjectFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<AiCharacterFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ProjectileFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<VFXFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ThrowableFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<EquipmentFactory>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RendererFactory>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}

}