using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Banner;
using Common.SpawnPoint;
using Core.Factory;
using Core.InputSystem;
using Core.Services;
using Cysharp.Threading.Tasks;
using GameStateMachine.States.Impl.Project;
using MessagePipe;
using SharedUtils;
using Ui.Sandbox;
using Ui.Sandbox.CharacterMenu;
using Ui.Sandbox.SettingsMenu;
using Ui.Sandbox.ShopMenu;
using Ui.Sandbox.SpawnMenu;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using VContainerUi.Messages;

namespace Sandbox.Initializers
{

	public class SandboxInitializer : IAsyncStartable
	{
		private readonly IPublisher<MessageOpenWindow> _publisher;
		private readonly IAdapterStrategyFactory _adapterStrategyFactory;
		private readonly IMapService _mapService;
		private readonly IObjectResolver _resolver;
		private readonly IPublisher<SceneLoadedMessage> _sceneLoadedMsg;
		private readonly IPlayerSpawnService _spawnService;
		private readonly ISpawnPointProvider _spawnPointProvider;
		private readonly CharacterMenuController _characterMenuController;
		private readonly IObjectFactory _objectFactory;
		private readonly IMenuPanelService _menuPanelService;
		private readonly IBannerService _bannerService;

		public SandboxInitializer(
			IPublisher<MessageOpenWindow> publisher,
			IAdapterStrategyFactory adapterStrategyFactory,
			IMapService mapService,
			IObjectResolver resolver, 
			IPublisher<SceneLoadedMessage> sceneLoadedMsg,
			IPlayerSpawnService spawnService,
			ISpawnPointProvider spawnPointProvider,
			CharacterMenuController characterMenuController,
			IObjectFactory objectFactory,
			IMenuPanelService menuPanelService,
			IBannerService bannerService)
		{
			_publisher = publisher;
			_adapterStrategyFactory = adapterStrategyFactory;
			_mapService = mapService;
			_resolver = resolver;
			_sceneLoadedMsg = sceneLoadedMsg;
			_spawnService = spawnService;
			_spawnPointProvider = spawnPointProvider;
			_characterMenuController = characterMenuController;
			_objectFactory = objectFactory;
			_menuPanelService = menuPanelService;
			_bannerService = bannerService;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			_publisher.Publish(new MessageOpenWindow(nameof(SandboxMainWindow)));
#if UNITY_EDITOR
			Application.targetFrameRate = -1;
#endif
			_objectFactory.Holder.gameObject.SetActive(false);
			await UniTask.Yield(cancellation);
			
			foreach (var mapExterior in _mapService.MapObject.MapExteriorGroups)
			{
				await UniTask.Yield(cancellation);
				_resolver.Inject(mapExterior);
				await mapExterior.CreateObjectsAsync();
			}
			
			foreach (var injectable in _mapService.MapObject.Injectables)
			{
				_resolver.Inject(injectable);
				await UniTask.Yield(cancellation);
			}

			var audioPlayer = new AudioPlayer[30];
			for (int i = 0; i < audioPlayer.Length; i++)
			{
				audioPlayer[i] = LucidAudio.PlaySE(null, 0);
			}
			for (int i = 0; i < audioPlayer.Length; i++)
			{
				audioPlayer[i].Stop();
			}
			await UniTask.Yield(cancellation);

#if UNITY_EDITOR
			var objs = GameObject.FindGameObjectsWithTag("DebugPoint");
			foreach (var obj in objs)
			{
				obj.GetComponent<SpawnPoint>().Create(_adapterStrategyFactory);
			}
#endif
			_spawnService.CreatePlayerContextAt(_characterMenuController.GetSavedChar(), _spawnPointProvider.GetInitialSpawnPoint());
			await UniTask.Yield(cancellation);
			_objectFactory.Holder.gameObject.SetActive(true);
			_menuPanelService.SelectMenu(nameof(CharacterMenuWindow));
			await UniTask.Yield(cancellation);
			_menuPanelService.SelectMenu(nameof(SpawnMenuWindow));
			await UniTask.Yield(cancellation);
			_menuPanelService.SelectMenu(nameof(ShopMenuWindow));
			await UniTask.Yield(cancellation);
			_menuPanelService.SelectMenu(nameof(SettingsMenuWindow));
			await UniTask.Yield(cancellation);
			_menuPanelService.CloseMenu();
			await UniTask.Delay(0.5f.ToSec(), cancellationToken: cancellation);

			_sceneLoadedMsg.Publish(new SceneLoadedMessage(SceneLoaderService.SANDBOX));
			
			await _bannerService.ShowBanners(cancellation);
		}
	}
}