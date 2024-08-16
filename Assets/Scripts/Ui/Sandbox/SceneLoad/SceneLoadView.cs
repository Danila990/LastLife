using System;
using System.Collections.Generic;
using Adv.Services.Interfaces;
using Core.Services;
using MessagePipe;
using Sirenix.OdinInspector;
using Ticket;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Abstraction;
using VContainerUi.Messages;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SceneLoad
{
	public class SceneLoadView : UiView
	{
		public SceneData[] SceneLoadVariants;
		
		public Transform Content;
		public SceneLoadUiElement SceneLoadUiElementPrefab;
	}

	[Serializable]
	public struct SceneData
	{
		[ValueDropdown("@SceneUtil.GetAllBuildScenes()")]
		public string SceneId;
		public string SceneTextName;
		public Sprite SceneIcon;
	}

	public class SceneLoadController : UiController<SceneLoadView>, IStartable, IDisposable
	{
		private readonly ISceneLoaderService _sceneLoaderService;
		private readonly IPublisher<MessageBackWindow> _backWindowPublisher;
		private readonly IAdvService _advService;
		private readonly ITicketService _ticketService;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly List<SceneLoadUiElement> _uiElements = new List<SceneLoadUiElement>();
		private string _sceneNameToLoad;

		public SceneLoadController(
			ISceneLoaderService sceneLoaderService, 
			IPublisher<MessageBackWindow> backWindowPublisher,
			IAdvService advService,
			ITicketService ticketService)
		{
			_sceneLoaderService = sceneLoaderService;
			_backWindowPublisher = backWindowPublisher;
			_advService = advService;
			_ticketService = ticketService;
		}
		
		public void Start()
		{
			foreach (var viewSceneLoadVariant in View.SceneLoadVariants)
			{
				var uiElement = Object.Instantiate(View.SceneLoadUiElementPrefab, View.Content);
				uiElement.Init(viewSceneLoadVariant);
				uiElement.LoadButton.OnClickAsObservable().SubscribeWithState(viewSceneLoadVariant.SceneId, OnClick).AddTo(_compositeDisposable);
				uiElement.Image.OnPointerClickAsObservable().SubscribeWithState(viewSceneLoadVariant.SceneId, OnClickImage).AddTo(_compositeDisposable);
				uiElement.TicketButton.OnClickAsObservable().SubscribeWithState(viewSceneLoadVariant.SceneId,OnClickTicket).AddTo(_compositeDisposable);
				_uiElements.Add(uiElement);
			}
		}
		
		private void OnClickTicket(Unit arg1, string sceneName)
		{
			_sceneNameToLoad = sceneName;
			_ticketService.TryUseTicket(Load, $"Train:{sceneName}");
		}

		public override void OnShow()
		{
			var sceneName = SceneManager.GetActiveScene().name;
			foreach (var sceneLoadUiElement in _uiElements)
			{
				sceneLoadUiElement.gameObject.SetActive(sceneName != sceneLoadUiElement.SceneId);
			}
		}

		private void OnClick(Unit arg1, string sceneName)
		{
			_sceneNameToLoad = sceneName;
			_advService.RewardRequest(Load, $"Train:{sceneName}");
		}

		private void OnClickImage(PointerEventData arg1, string sceneName)
		{
			_sceneNameToLoad = sceneName;
			_advService.RewardRequest(Load, $"Train:{sceneName}");
		}
		
		private void Load()
		{
			Load(_sceneNameToLoad);
		}
		
		private void Load(string sceneName)
		{
			_backWindowPublisher.BackWindow(UiScope.Project);
			//_sceneLoaderService.LoadSceneAsync(sceneName).Forget();
		}

		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}