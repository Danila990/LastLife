using System;
using System.Threading;
using Adv.Messages;
using Analytic;
using MessagePipe;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;
using VContainer.Unity;

namespace Adv.Providers
{
	public class MockAdvProvider : IAdvProvider, IDisposable
	{
		//private readonly IPauseService _pauseService;
		private readonly IAnalyticService _analyticService;
		private readonly IPublisher<MessageHideAd> _hideAdWindow;
		private readonly IPublisher<MessageShowAd> _showAd;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable(); 
		private Action _interstitialCallback;
		private Action _rewardCallback;

		public bool InterstitialIsLoaded => true;
		public bool RewardIsLoaded => true;
		private readonly ReactiveCommand _showRewarded = new ReactiveCommand();
		private readonly ReactiveCommand _hideRewarded = new ReactiveCommand();

		public IReactiveCommand<Unit> ShowedReward => _showRewarded;
		public IReactiveCommand<Unit> HidedReward => _hideRewarded;


		public MockAdvProvider(
			IAnalyticService analyticService,
			IPublisher<MessageHideAd> hideAdWindow, 
			IPublisher<MessageShowAd> showAd)
		{
			// _pauseService = pauseService;
			_analyticService = analyticService;
			_hideAdWindow = hideAdWindow;
			_showAd = showAd;
		}
        
		public bool ShowInterstitial(Action callback, string interstitialType)
		{
			_interstitialCallback = callback;
	
			_showRewarded.Execute();
			_showAd.Publish(default);

			_hideAdWindow.Publish(default);
			_hideRewarded.Execute();
			_analyticService.SendEvent($"Interstitial:{interstitialType}");
			_interstitialCallback?.Invoke();
			_interstitialCallback = null;
			return true;
		}

		public bool ShowReward(Action onReward, string rewardType)
		{
			_showRewarded.Execute();
			_showAd.Publish(default);
			_hideAdWindow.Publish(default);
			
			_hideRewarded.Execute();
			onReward?.Invoke();
			_analyticService.SendEvent($"Reward:{rewardType}");
			Debug.Log( "Reward Adv: ".SetColor("yellow") + $"[{("Reward:" + rewardType).SetColor("green")}]");
			return true;
		}
		
		public void Dispose()
		{
			_showRewarded?.Dispose();
			_hideRewarded?.Dispose();
			_compositeDisposable?.Dispose();
		}
	}
}