using System;
using UniRx;

namespace Adv.Services.Interfaces
{
	public interface IRemoveAdsService
	{
		/// <summary>
		/// Remove ads disable only Interstitial Ads
		/// If True ads Disabled
		/// </summary>
		bool IsRemoveAdsEnabled { get; }
		/// <summary>
		/// Used for Cheats
		/// If True ads Disabled
		/// </summary>
		bool IsRemoveRewardAds { get; set; }
		IObservable<bool> RemoveAdsBoughtObservable { get; }
		IReactiveCommand<Unit> OnStopAdv { get; }
		void RemoveAds(bool constantly);
	}
}