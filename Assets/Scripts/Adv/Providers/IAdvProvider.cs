using System;
using UniRx;

namespace Adv.Providers
{
    public interface IAdvProvider
    {
        bool ShowInterstitial(Action callback, string interstitialType);
        bool ShowReward(Action onReward, string rewardType);
        
        bool InterstitialIsLoaded { get; }
        bool RewardIsLoaded { get; }
        
        IReactiveCommand<Unit> ShowedReward { get; }
        IReactiveCommand<Unit> HidedReward { get; }
    }
}