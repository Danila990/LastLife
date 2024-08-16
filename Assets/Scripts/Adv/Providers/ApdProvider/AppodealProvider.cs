#if RELEASE_BRANCH
using System;
using System.Collections.Generic;
using Adv.Messages;
using Analytic;
using AppodealAppTracking.Unity.Api;
using AppodealAppTracking.Unity.Common;
using AppodealStack.Cmp;
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using MessagePipe;
using UniRx;
using UnityEngine;
using VContainer.Unity;
using VContainerUi.Messages;
using Object = UnityEngine.Object;

namespace Adv.Providers.ApdProvider
{
	public class AppodealProvider : 
		IAdvProvider,
		IStartable,
		IAppodealInitializationListener, 
		IRewardedVideoAdListener,
		IInterstitialAdListener, 
		IAppodealAppTrackingTransparencyListener
	{
		private readonly IAnalyticService _analyticService;
		private readonly IAppodealSettings _appodealSettings;
		private readonly IPublisher<MessageHideAd> _hideAd;
		private readonly IPublisher<MessageOpenWindow> _openWindow;
		private readonly IPublisher<MessageShowAd> _showAd;
		private const string APPODEAL_SKAD_ID = "Appodeal";
		private	string apiKey;
		
		private Action _rewardCallback;
		private Action _interstitialCallback;
		private readonly ReactiveCommand _showRewarded = new ReactiveCommand();
		private readonly ReactiveCommand _hideRewarded = new ReactiveCommand();
		public bool InterstitialIsLoaded => Appodeal.IsLoaded(AppodealAdType.Interstitial);
		public bool RewardIsLoaded => Appodeal.IsLoaded(AppodealAdType.RewardedVideo);
		public IReactiveCommand<Unit> ShowedReward => _showRewarded;
		public IReactiveCommand<Unit> HidedReward => _hideRewarded;

		public AppodealProvider(
			IAnalyticService analyticService, 
			IAppodealSettings appodealSettings,
			IPublisher<MessageHideAd> hideAd, 
			IPublisher<MessageOpenWindow> openWindow,
			IPublisher<MessageShowAd> showAd)
		{
			_analyticService = analyticService;
			_appodealSettings = appodealSettings;
			_hideAd = hideAd;
			_openWindow = openWindow;
			_showAd = showAd;
		}
		
		public void Start()
		{
			Appodeal.SetTesting(_appodealSettings.IsTestMode);
			Appodeal.SetRewardedVideoCallbacks(this);
			Appodeal.SetInterstitialCallbacks(this);
			Appodeal.SetLogLevel(_appodealSettings.LogLevel);
			
#if UNITY_ANDROID
			apiKey = _appodealSettings.ApiKeyAndroid;
			Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
#elif UNITY_IOS
			
			apiKey = _appodealSettings.ApiKeyIos;
			Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
			/*ConsentManager.Instance.OnConsentInfoUpdateFailed += (sender, args) =>
			{
				ShowAttAndInitApd();
			};

			ConsentManager.Instance.OnConsentInfoUpdateSucceeded += (sender, args) =>
			{
				ShowAttAndInitApd();
			};*/
			
			/*ConsentManager.Instance.RequestConsentInfoUpdate(new ConsentInfoParameters
			{
				AppKey = apiKey,
				IsUnderAgeToConsent = false,
				Sdk = "Appodeal",
				SdkVersion = Appodeal.GetNativeSDKVersion()
			});*/
#endif
		}

		private void ShowAttAndInitApd()
		{
			
#if UNITY_EDITOR
		Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
#else
		AppodealAppTrackingTransparency.RequestTrackingAuthorization(this);
#endif
			
		}

		public void OnInitializationFinished(List<string> errors)
		{
			if (errors == null)
			{
				//Appodeal.Cache(AppodealAdType.RewardedVideo);
			}
			else
			{
				foreach (var error in errors)
				{
					Debug.LogError(error);
				}
			}
		}
		
		public bool ShowInterstitial(Action callback, string interstitialType)
		{
			_interstitialCallback = callback;
			if (Appodeal.IsLoaded(AppodealAdType.Interstitial) && Appodeal.CanShow(AppodealAdType.Interstitial, "default"))
			{
				_showRewarded.Execute();
				_showAd.Publish(default);
				
				Appodeal.Show(AppodealShowStyle.Interstitial);
				_analyticService.SendEvent($"Interstitial:{interstitialType}");

				return true;
			}
			if (!Appodeal.IsAutoCacheEnabled(AppodealAdType.Interstitial))
			{
				Appodeal.Cache(AppodealAdType.Interstitial);
			}
			
			return false;
		}
		
		public bool ShowReward(Action callback, string rewardType)
		{
			_rewardCallback = callback;
			if (Appodeal.IsLoaded(AppodealAdType.RewardedVideo) && Appodeal.CanShow(AppodealAdType.RewardedVideo, "default"))
			{
				_showRewarded.Execute();
				_showAd.Publish(default);
				
				Appodeal.Show(AppodealShowStyle.RewardedVideo);
				if (!string.IsNullOrEmpty(rewardType))
				{
					_analyticService.SendEvent($"Reward:{rewardType}");
				}
				return true;
			}
			if (!Appodeal.IsAutoCacheEnabled(AppodealAdType.RewardedVideo))
			{
				Appodeal.Cache(AppodealAdType.RewardedVideo);
			}
			return false;
		}
		
		private void Callback(object obj)
		{
			if (obj is Action action)
			{
				action();
			}
		}
		
#region Reward
		public void OnRewardedVideoLoaded(bool isPrecache)
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Loaded, AdType.RewardedVideo,AdError.Undefined)), null);
		}
		public void OnRewardedVideoFailedToLoad()
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.FailedShow, AdType.RewardedVideo,AdError.Undefined)), null);
		}
		public void OnRewardedVideoShowFailed()
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.FailedShow, AdType.RewardedVideo,AdError.Undefined)), null);
		}
		public void OnRewardedVideoShown()
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Show, AdType.RewardedVideo,AdError.Undefined)), null);
		}
		public void OnRewardedVideoFinished(double amount, string currency)
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.RewardReceived, AdType.RewardedVideo,AdError.Undefined)), null);
			MainThreadDispatcher.Send(Callback, _rewardCallback);
			_rewardCallback = null;
			//EndAd();
		}
		
		public void OnRewardedVideoClosed(bool finished)
		{
			MainThreadDispatcher.Send(o => _hideAd.Publish(default), null);
		}
		
		public void OnRewardedVideoExpired()
		{
			//Debug.Log("[APDUnity] [Callback] OnRewardedVideoExpired()");
		}
		public void OnRewardedVideoClicked()
		{
			var args = new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Clicked, AdType.RewardedVideo,
				AdError.Undefined);
			args.ECpm = Appodeal.GetPredictedEcpm(AppodealAdType.RewardedVideo);
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Clicked, AdType.RewardedVideo,AdError.Undefined)), null);
		}
#endregion

#region Interstitial
		public void OnInterstitialLoaded(bool isPrecache)
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Loaded, AdType.Interstitial,AdError.Undefined)), null);
		}
		
		public void OnInterstitialFailedToLoad()
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.FailedShow, AdType.Interstitial,AdError.Undefined)), null);
		}
		
		public void OnInterstitialShowFailed()
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.FailedShow, AdType.Interstitial,AdError.Undefined)), null);
			MainThreadDispatcher.Send(o => _hideAd.Publish(default), null);
		}
		
		public void OnInterstitialShown()
		{
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Show, AdType.Interstitial,AdError.Undefined)), null);
		}
		
		public void OnInterstitialClosed()
		{
			MainThreadDispatcher.Send(Callback, _interstitialCallback);
			MainThreadDispatcher.Send(o => _hideAd.Publish(default), null);
		}
		
		public void OnInterstitialClicked()
		{
			var args = new ADEventArgs(APPODEAL_SKAD_ID, "", AdEventType.Clicked, AdType.Interstitial,
				AdError.Undefined)
			{
				ECpm = Appodeal.GetPredictedEcpm(AppodealAdType.Interstitial)
			};
			MainThreadDispatcher.Send(o => _analyticService.SendADEvent(args), null);
		}
		
		public void OnInterstitialExpired()
		{
		}
#endregion
		
#region App Tracking Transparency
		public void AppodealAppTrackingTransparencyListenerNotDetermined()
		{
			Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
		}

		public void AppodealAppTrackingTransparencyListenerRestricted()
		{
			Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
		}

		public void AppodealAppTrackingTransparencyListenerDenied()
		{
			Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
		}

		public void AppodealAppTrackingTransparencyListenerAuthorized()
		{
			Appodeal.Initialize(apiKey, _appodealSettings.AdIntValue, this);
		}
#endregion
	}
}

#endif