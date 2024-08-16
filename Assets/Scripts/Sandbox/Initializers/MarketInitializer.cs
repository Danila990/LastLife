using System.Threading;
using AgeConversation;
using Banner;
using Core.InputSystem;
using Core.Quests.Tips;
using Core.Services;
using Cysharp.Threading.Tasks;
using GameStateMachine.States.Impl.Project;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using Ui.Sandbox;
using Ui.Sandbox.CharacterMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VContainerUi.Messages;

namespace Sandbox.Initializers
{
	public class MarketInitializer : IStartable, IAsyncStartable
	{
		private readonly IPublisher<MessageOpenWindow> _publisher;
		private readonly IMapService _mapService;
		private readonly IObjectResolver _resolver;
		private readonly IPublisher<SceneLoadedMessage> _sceneLoadedMsg;
		private readonly IPlayerSpawnService _spawnService;
		private readonly ISpawnPointProvider _spawnPointProvider;
		private readonly IQuestTipService _questTipService;
		private readonly CharacterMenuController _characterMenuController;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly IBannerService _bannerService;

		public MarketInitializer(
			IPublisher<MessageOpenWindow> publisher,
			IMapService mapService,
			IObjectResolver resolver, 
			IPublisher<SceneLoadedMessage> sceneLoadedMsg,
			IPlayerSpawnService spawnService,
			ISpawnPointProvider spawnPointProvider,
			IQuestTipService questTipService,
			CharacterMenuController characterMenuController,
			IPlayerPrefsManager playerPrefsManager,
			IBannerService bannerService)
		{
			_publisher = publisher;
			_mapService = mapService;
			_resolver = resolver;
			_sceneLoadedMsg = sceneLoadedMsg;
			_spawnService = spawnService;
			_spawnPointProvider = spawnPointProvider;
			_questTipService = questTipService;
			_characterMenuController = characterMenuController;
			_playerPrefsManager = playerPrefsManager;
			_bannerService = bannerService;
		}
	
		public void Start()
		{
			_publisher.Publish(new MessageOpenWindow(nameof(SandboxMainWindow)));
#if UNITY_EDITOR
			Application.targetFrameRate = -1;
#endif
			CreateObjects();
		}
	
		private void CreateObjects()
		{
			foreach (var mapExterior in _mapService.MapObject.MapExteriorGroups)
			{
				_resolver.Inject(mapExterior);
				mapExterior.CreateObjects();
			}
		
			foreach (var injectable in _mapService.MapObject.Injectables)
			{
				_resolver.Inject(injectable);
			}
			
			foreach (var questTip in _mapService.MapObject.QuestTipContexts)
			{
				_questTipService.AddTip(questTip);
			}
		}
		
		public async UniTask StartAsync(CancellationToken cancellation)
		{
			_spawnService.CreatePlayerContextAt(_characterMenuController.GetSavedChar(), _spawnPointProvider.GetInitialSpawnPoint());
			_sceneLoadedMsg.Publish(new SceneLoadedMessage(SceneLoaderService.MARKET));

			await _bannerService.ShowBanners(cancellation);
			Debug.Log("Hide all banners");
		}
	}
}