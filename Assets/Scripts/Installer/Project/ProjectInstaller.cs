using Core.Factory.Ui;
using Core.Fuel;
using Core.Pause;
using Core.Quests.Messages;
using Core.ResourcesSystem;
using Core.Services;
using Core.Services.Input;
using LostConnection;
using RemoteServer;
using SharedUtils.PlayerPrefs;
using SharedUtils.PlayerPrefs.Impl;
using Utils;
using VContainer;
using VContainer.Extensions;

namespace Installer.Project
{
	public class ProjectInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			var token = new ProjectCancellationToken(destroyCancellationToken);
			builder.RegisterInstance(token);
			RegisterServices(builder);
		}
		
		private void RegisterServices(IContainerBuilder builder)
		{
#region PAUSE
			builder.Register<PauseService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<AdvPauseService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<InternetConnectionChecker<LostConnectionWindow>>(Lifetime.Singleton).AsImplementedInterfaces();
#endregion
			
			builder.Register<ControlFreakInputService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<InMemoryPlayerPrefsManager>(Lifetime.Singleton).AsSelf();
			
			builder.Register<ItemStorage>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ItemUnlockService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RemoveAdsShopHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<AudioService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SceneLoaderService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PersistentPlayerPrefsManager>(Lifetime.Singleton).As<IPlayerPrefsManager>();
			builder.Register<QuestMessageSender>(Lifetime.Singleton).AsImplementedInterfaces();

			//Factories
			builder.Register<ShopPresentersFactory>(Lifetime.Singleton).As<IShopPresentersFactory>();
			
			builder.Register<RemoteServerRequester>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RemoteAccess>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<FuelService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ResourcesService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}