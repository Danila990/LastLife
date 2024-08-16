using System;
using System.Threading;
using Core.Pause;
using Core.Services;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Ui.Sandbox.LoadScreen;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Messages;

namespace GameStateMachine.States.Impl.Project
{
	public class ProjectLoadSceneState : IPayloadAsyncGameState<string>
	{
		private IGameStateMachine _stateMachine;
		private readonly ISceneLoaderService _sceneLoaderService;
		private readonly ISubscriber<SceneLoadedMessage> _sceneLoadedMsg;
		private readonly IPublisher<MessageOpenWindow> _openWindowPub;
		private readonly IPublisher<MessageBackWindow> _backWindowPub;
		private readonly IPauseService _pauseService;

		public ProjectLoadSceneState(
			ISceneLoaderService sceneLoaderService, 
			ISubscriber<SceneLoadedMessage> sceneLoadedMsg,
			IPublisher<MessageOpenWindow> openWindowPub,
			IPublisher<MessageBackWindow> backWindowPub,
			IPauseService pauseService
			)
		{
			_sceneLoaderService = sceneLoaderService;
			_sceneLoadedMsg = sceneLoadedMsg;
			_openWindowPub = openWindowPub;
			_backWindowPub = backWindowPub;
			_pauseService = pauseService;
		}
		
		public async UniTaskVoid EnterState(CancellationToken token, string payload)
		{
			_openWindowPub.OpenWindow<LoadScreenWindow>(UiScope.Project);

			await _sceneLoaderService.LoadSceneAdditiveAsync(payload, showLoadingScreenManually: true);
			var activeScene = SceneManager.GetActiveScene();
			var scope = LifetimeScope.Find<LifetimeScope>(activeScene);
			scope.Build();
			await _sceneLoadedMsg.FirstAsync(token);
			await UniTask.DelayFrame(2, cancellationToken: token);
			
			_backWindowPub.Publish(new MessageBackWindow(UiScope.Project));
		}
		
		public void ExitState()
		{
			
		}
		
		public void SetStateMachine(IGameStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}
		
		public void EnterState()
			=> throw new NotImplementedException();
	}
}