using System;
using Adv.Messages;
using Adv.Providers;
using Adv.Services.Interfaces;
using MessagePipe;
using UniRx;
using UnityEngine;

namespace Adv.Services
{
	public class SimpleAdvService : IAdvService
	{
		private readonly IAdvProvider _advProvider;
		private readonly IRemoveAdsService _removeAdsService;
		private readonly IPublisher<MessageShowAdWindow> _showAdWindowPublisher;
		private readonly IPublisher<MessageHideAd> _hideAdPub;

		public IObservable<Unit> OnShownRewardVideo => _advProvider.ShowedReward;

		public SimpleAdvService(
			IAdvProvider advProvider, 
			IRemoveAdsService removeAdsService,
			IPublisher<MessageShowAdWindow> showAdWindowPublisher,
			IPublisher<MessageHideAd> hideAdPub
			)
		{
			_advProvider = advProvider;
			_removeAdsService = removeAdsService;
			_showAdWindowPublisher = showAdWindowPublisher;
			_hideAdPub = hideAdPub;
		}
		
		public void RewardRequest(Action onReward, string reward)
		{
			if(_removeAdsService.IsRemoveRewardAds)
			{
#if UNITY_EDITOR
				Debug.Log("Adv Skipped by Remove Ads");
#endif
				onReward?.Invoke();
				return;
			}
			
			_advProvider.ShowReward(onReward, reward);
		}
		
		public void InterstitialRequest(Action onComplete, string interstitialMeta)
		{
			if(_removeAdsService.IsRemoveAdsEnabled)
			{
#if UNITY_EDITOR
				Debug.Log("Adv Skipped by Remove Ads");
#endif
				onComplete?.Invoke();
				return;
			}
			_advProvider.ShowInterstitial(onComplete, interstitialMeta);
		}
		
		public void InterstitialRequestWithWindow(Action onComplete, string interstitialMeta)
		{
			if(_removeAdsService.IsRemoveAdsEnabled)
			{
#if UNITY_EDITOR
				Debug.Log("Adv Skipped by Remove Ads");
#endif
				onComplete?.Invoke();
				return;
			}
			_showAdWindowPublisher.Publish(new MessageShowAdWindow(ShowInter, onComplete, interstitialMeta));
		}
		
		private void ShowInter(Action onComplete, string interstitialMeta)
		{
			var interShown = _advProvider.ShowInterstitial(onComplete, interstitialMeta);
			if (!interShown)
			{
				_hideAdPub.Publish(new MessageHideAd());
			}
		}
	}
}