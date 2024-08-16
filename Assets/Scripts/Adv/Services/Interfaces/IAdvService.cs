using System;
using UniRx;

namespace Adv.Services.Interfaces
{
	public interface IAdvService
	{
		IObservable<Unit> OnShownRewardVideo { get; }
		
		void RewardRequest(Action onReward, string reward);
		void InterstitialRequest(Action onComplete, string interstitialMeta);
		void InterstitialRequestWithWindow(Action onComplete, string interstitialMeta);
	}
}