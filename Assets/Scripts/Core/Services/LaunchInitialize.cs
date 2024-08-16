using System;
using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameStateMachine.States.Impl.Project;
using LitMotion;
using MessagePipe;
using Shop;
using Shop.Abstract;
using UniRx;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using VContainer.Unity;

namespace Core.Services
{
	public class LaunchInitialize : IAsyncStartable
	{
		private readonly IAPManager _iapManager;
		private readonly IInAppDatabase _inAppDatabase;
		private readonly IPublisher<SceneLoadedMessage> _sceneLoadedMsg;

		private const string ENVIRONMENT =
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			"editor";
#else
			"production";
#endif
		
		public LaunchInitialize(
			IAPManager iapManager,
			IInAppDatabase inAppDatabase,
			IPublisher<SceneLoadedMessage> sceneLoadedMsg)
		{
			_iapManager = iapManager;
			_inAppDatabase = inAppDatabase;
			_sceneLoadedMsg = sceneLoadedMsg;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			InitializePlugins();
			await InitializePurchasing();
			await UniTask.NextFrame(cancellationToken: cancellation);
			_sceneLoadedMsg.Publish(new SceneLoadedMessage(SceneLoaderService.LAUNCH_SCENE));
		}
		
		private async UniTask InitializePurchasing()
		{
			var options = new InitializationOptions()
				.SetEnvironmentName(ENVIRONMENT);
			
			try
			{
				await UnityServices.InitializeAsync(options);
				_iapManager.Initialize(_inAppDatabase.GetProducts());
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);	
			}
		}

		private static void InitializePlugins()
		{
			MainThreadDispatcher.Initialize();
			DOTween.Init();
		}
	}
}