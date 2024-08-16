using System;
using System.Threading;
using Analytic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SharedUtils;
using Ui.Sandbox.LoadScreen;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using VContainerUi;
using VContainerUi.Messages;

namespace Core.Services
{
	public class SceneLoaderService : ISceneLoaderService, IDisposable
	{
		private readonly IAnalyticService _analyticService;
		private readonly IPublisher<MessageOpenWindow> _openWindowPub;
		private readonly IPublisher<MessageBackWindow> _backWindowPub;
		public const string LAUNCH_SCENE = "Launch";
		public const string SANDBOX = "Sandbox-Base";
		public const string MARKET = "Market";
		public const string TEST_SCENE = "Test_Scene";

		private readonly ReactiveCommand<SceneChangeEventData> _beforeSceneChange = new ReactiveCommand<SceneChangeEventData>();
		private readonly ReactiveCommand<SceneChangeEventData> _afterSceneChange = new ReactiveCommand<SceneChangeEventData>();
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		
		private readonly LoadScreenController _loadScreenController;
		private readonly ProjectCancellationToken _projectCancellationToken;
		private IDisposable _disposable;

		public string LastScene { get; private set; } = SANDBOX;
		public IReactiveCommand<SceneChangeEventData> BeforeSceneChange => _beforeSceneChange;
		public IReactiveCommand<SceneChangeEventData> AfterSceneChange => _afterSceneChange;

		public SceneLoaderService(
			IAnalyticService analyticService,
			IPublisher<MessageOpenWindow> openWindowPub,
			IPublisher<MessageBackWindow> backWindowPub,
			LoadScreenController loadScreenController,
			ProjectCancellationToken projectCancellationToken
			)
		{
			_analyticService = analyticService;
			_openWindowPub = openWindowPub;
			_backWindowPub = backWindowPub;
			_loadScreenController = loadScreenController;
			_projectCancellationToken = projectCancellationToken;
		}
		
		public void LoadLastScene(bool saveLastScene = true)
		{
			LoadScene(LastScene, saveLastScene);
		}

		public void LoadScene(string scene, bool saveLastScene = true)
		{
			var currentScene = SceneManager.GetActiveScene().name;
			_beforeSceneChange.Execute(new SceneChangeEventData(currentScene, scene));
			_analyticService.SendEvent($"LoadScene:{scene}");
			
			if (saveLastScene)
				LastScene = currentScene;
			
			SceneManager.LoadScene(scene);
			_afterSceneChange.Execute(new SceneChangeEventData(currentScene, scene));
		}
		
		public void ReloadScene()
		{
			var currentScene = SceneManager.GetActiveScene().name;
			_beforeSceneChange.Execute(new SceneChangeEventData(currentScene, currentScene));
			
			SceneManager.LoadScene(currentScene);
			_afterSceneChange.Execute(new SceneChangeEventData(currentScene, currentScene));
		}

		public async UniTaskVoid LoadSceneAsync(string scene, bool saveLastScene = true, bool showLoadingScreenManually = false)
		{
			var ct = _cancellationTokenSource.Token;
			
			if (!showLoadingScreenManually)
				_openWindowPub.OpenWindow<LoadScreenWindow>(UiScope.Project);

			_analyticService.SendEvent($"LoadScene:{scene}");
			var currentScene = SceneManager.GetActiveScene().name;
			if (saveLastScene)
				LastScene = currentScene;
			
			_beforeSceneChange.Execute(new SceneChangeEventData(currentScene, scene));
			
			await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);
			
			await SceneManager
				.LoadSceneAsync(scene)
				.ToUniTask(cancellationToken: ct, progress:_loadScreenController);

			if (ct.IsCancellationRequested)
				return;
			
			if (!showLoadingScreenManually)
				_backWindowPub.Publish(new MessageBackWindow(UiScope.Project));
			
			_afterSceneChange.Execute(new SceneChangeEventData(currentScene, scene));
		}
		
		public async UniTask LoadSceneAdditiveAsync(string scene, bool saveLastScene = true, bool showLoadingScreenManually = false)
		{
			var ct = _cancellationTokenSource.Token;
			
			if (!showLoadingScreenManually)
				_openWindowPub.OpenWindow<LoadScreenWindow>(UiScope.Project);

			_analyticService.SendEvent($"LoadScene:{scene}");
			var currentScene = SceneManager.GetActiveScene().name;
			if (saveLastScene)
				LastScene = currentScene;
			
			_beforeSceneChange.Execute(new SceneChangeEventData(currentScene, scene));
			
			await UniTask.Delay(.1f.ToSec(), DelayType.UnscaledDeltaTime, cancellationToken: ct);
			if (ct.IsCancellationRequested)
				return;
			
			await SceneManager
				.LoadSceneAsync(scene, LoadSceneMode.Additive)
				.ToUniTask(cancellationToken: ct, progress:_loadScreenController);

			var loadedScene = SceneManager.GetSceneByName(scene);
			SceneManager.SetActiveScene(loadedScene);
			
			if (ct.IsCancellationRequested)
				return;
			
			await SceneManager
				.UnloadSceneAsync(currentScene)
				.ToUniTask(cancellationToken: ct);

			await Resources
				.UnloadUnusedAssets()
				.ToUniTask(cancellationToken: ct);
			
			if (ct.IsCancellationRequested)
				return;
			
			if (!showLoadingScreenManually)
				_backWindowPub.Publish(new MessageBackWindow(UiScope.Project));
			
			_afterSceneChange.Execute(new SceneChangeEventData(currentScene, scene));
		}
		
		public void Dispose()
		{
			_beforeSceneChange?.Dispose();
			_afterSceneChange?.Dispose();
			_disposable?.Dispose();
			
			if(!_cancellationTokenSource.IsCancellationRequested)
				_cancellationTokenSource?.Cancel();
			
			_cancellationTokenSource?.Dispose();
		}
	}
	
	public interface ISceneLoaderService
	{
		string LastScene { get; }
		IReactiveCommand<SceneChangeEventData> BeforeSceneChange { get; }
		IReactiveCommand<SceneChangeEventData> AfterSceneChange { get; }
		void LoadLastScene(bool saveLastScene = true);
		void LoadScene(string scene, bool saveLastScene = true);
		void ReloadScene();
		UniTaskVoid LoadSceneAsync(string scene, bool saveLastScene = true, bool showLoadingScreenManually = false);
		UniTask LoadSceneAdditiveAsync(string scene, bool saveLastScene = true, bool showLoadingScreenManually = false);
	}

	public readonly struct SceneChangeEventData
	{
		public readonly string SceneFrom;
		public readonly string LoadedScene;
		public bool IsReload => SceneFrom == LoadedScene;
		
		public SceneChangeEventData(string sceneFrom, string loadedScene)
		{
			SceneFrom = sceneFrom;
			LoadedScene = loadedScene;
		}
	}
}