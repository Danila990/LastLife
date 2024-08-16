using System;
using System.Threading;
using Core.Services;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using VContainer.Unity;

namespace GameStateMachine.States.Impl.Project
{
	public class ProjectInitializeState : IGameState, IInitializable, IDisposable
	{
		private readonly ISceneLoaderService _sceneLoaderService;
		private readonly ISubscriber<SceneLoadedMessage> _sceneLoadedMsg;
		private IGameStateMachine _stateMachine;
		private readonly IPlayerPrefsManager _prefsManager;
		private readonly CompositeDisposable _disposable;
		private string _loadFromScene;

		private readonly string _lastScenePrefsKey = "last_played_scene";
		public const string SESSION_COUNT = "SESSION_COUNT";
		
		public ProjectInitializeState(
			ISceneLoaderService sceneLoaderService,
			IPlayerPrefsManager prefsManager,
			ISubscriber<SceneLoadedMessage> sceneLoadedMsg
			)
		{
			_sceneLoaderService = sceneLoaderService;
			_prefsManager = prefsManager;
			_sceneLoadedMsg = sceneLoadedMsg;
			_disposable = new CompositeDisposable();
		}

		private void OnSceneLoaded(SceneLoadedMessage sceneLoadedMessage)
		{
			_prefsManager.SetValue(SESSION_COUNT, _prefsManager.GetValue(SESSION_COUNT, 0) + 1);
			_stateMachine.ChangeStateAsync<ProjectLoadSceneState, string>(_loadFromScene);
		}
		
		private void OnBeforeSceneChange(SceneChangeEventData eventData)
		{
			_prefsManager.SetValue(_lastScenePrefsKey, eventData.LoadedScene);
		}
		
		public void Initialize()
		{
			_sceneLoadedMsg.Subscribe(OnSceneLoaded, message => message.SceneName == SceneLoaderService.LAUNCH_SCENE).AddTo(_disposable);
			_sceneLoaderService.BeforeSceneChange.Subscribe(OnBeforeSceneChange).AddTo(_disposable);
#if UNITY_EDITOR
			_loadFromScene = SceneManager.GetActiveScene().name;
			if (_loadFromScene is SceneLoaderService.SANDBOX or SceneLoaderService.MARKET or SceneLoaderService.TEST_SCENE)
			{
				_sceneLoaderService.LoadScene(SceneLoaderService.LAUNCH_SCENE);
			}
			else if (_loadFromScene == SceneLoaderService.LAUNCH_SCENE)
			{
				var lastPlayedScene = _prefsManager.GetValue<string>(_lastScenePrefsKey, SceneLoaderService.SANDBOX);
				_loadFromScene = SceneUtil.IsGameSceneExist(lastPlayedScene) ? lastPlayedScene : SceneLoaderService.SANDBOX;
			}
#else
			var lastPlayedScene = _prefsManager.GetValue<string>(_lastScenePrefsKey, SceneLoaderService.SANDBOX);
			_loadFromScene = string.IsNullOrEmpty(lastPlayedScene) ? SceneLoaderService.SANDBOX : lastPlayedScene;
#endif
		}

		public void EnterState()
		{
			InitializeFromLaunchScene();
		}
		
		private void InitializeFromLaunchScene()
		{
			/*var activeScene = SceneManager.GetSceneByName(SceneLoaderService.LAUNCH_SCENE);
			var scope = LifetimeScope.Find<LifetimeScope>(activeScene);
			scope.Build();*/
		}

		public void ExitState()
		{
			_disposable?.Dispose();
		}
		
		public void SetStateMachine(IGameStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}

	public readonly struct SceneLoadedMessage
	{
		public string SceneName { get; }
		public SceneLoadedMessage(string sceneName)
		{
			SceneName = sceneName;
		}
	} 
}