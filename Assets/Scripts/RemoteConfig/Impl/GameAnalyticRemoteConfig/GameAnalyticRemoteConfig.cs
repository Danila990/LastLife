using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
#if GAME_ANALYTIC_INTEGRATION
using GameAnalyticsSDK;
#endif
using VContainer.Unity;

namespace RemoteConfig.Impl.GameAnalyticRemoteConfig
{
	public class GameAnalyticRemoteConfig : IRemoteConfigAdapter, IAsyncStartable
	{
		private readonly ReactiveProperty<bool> _onConfigUpdated = new ReactiveProperty<bool>();
		public IObservable<bool> OnConfigUpdated => _onConfigUpdated;
		
		public string GetValue(string key, string defaultValue)
		{
#if GAME_ANALYTIC_INTEGRATION
			if (!GameAnalytics.IsRemoteConfigsReady()) 
				return defaultValue;

			var result = GameAnalytics.GetRemoteConfigsValueAsString(key, defaultValue);
			Debug.Log("[Remote Config] Get Value: " + result);
			return result;
#else
			return defaultValue;
#endif
		}
		
		public void Initialize()
		{
#if GAME_ANALYTIC_INTEGRATION
			Debug.Log("[Remote Config] Sub Remote Config");
			GameAnalytics.OnRemoteConfigsUpdatedEvent += OnRemoteConfigsUpdated;
#endif
		}
		

		public async UniTask StartAsync(CancellationToken cancellation)
		{
#if GAME_ANALYTIC_INTEGRATION
			var curDelay = 250;
			while (!cancellation.IsCancellationRequested)
			{
				Debug.Log("TryFetch " + curDelay);
				await UniTask.Delay(curDelay, cancellationToken: cancellation);
				if (GameAnalytics.IsRemoteConfigsReady())
				{
					OnRemoteConfigsUpdated();
					return;
				}
				curDelay = Mathf.Min(10000, curDelay * 2);
			}
#endif
		}
		
		private void OnRemoteConfigsUpdated()
		{
			Debug.Log("[Remote Config] Updated");
			_onConfigUpdated.SetValueAndForceNotify(true);
		}
	}
}