using System;
using Adv.Services.Interfaces;
using SharedUtils.PlayerPrefs;
using UniRx;

namespace Adv.Services
{
	public class RemoveAdsService : IRemoveAdsService, IDisposable
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly ReactiveCommand _stopAdv = new ReactiveCommand();
		private readonly BoolReactiveProperty _removeAdsIsBought;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		
		public bool RemoveAdsIsBought => _removeAdsIsBought.Value;
		public bool IsRemoveAdsEnabled => RemoveAdsIsBought || _isRemoveAdsBySubscription || _playerPrefsManager.GetValue("SkipInterstitial", false);
		public bool IsRemoveRewardAds { get; set; }
		public IReactiveCommand<Unit> OnStopAdv => _stopAdv;
		public IObservable<bool> RemoveAdsBoughtObservable => _removeAdsIsBought;
		
		private const string REMOVE_ADS = "removeads";
		private bool _isRemoveAdsBySubscription;

		public RemoveAdsService(IPlayerPrefsManager playerPrefsManager)
		{
			_playerPrefsManager = playerPrefsManager;
			_removeAdsIsBought = new BoolReactiveProperty(_playerPrefsManager.GetValue(REMOVE_ADS, false));
		}
		
		public void RemoveAds(bool constantly)
		{
			_stopAdv.Execute();

			if (constantly)
			{
				BoughtRemoveAds();
			}
		}

		private void BoughtRemoveAds()
		{
			_playerPrefsManager.SetValue<bool>(REMOVE_ADS, true);
			_removeAdsIsBought.Value = true;
		}
		
		public void ReApplyAds()
		{
			_playerPrefsManager.SetValue<bool>(REMOVE_ADS, false);
			_removeAdsIsBought.Value = false;
			IsRemoveRewardAds = false;
		}
		
		private void OnSubscription(bool isSub)
		{
			if (isSub)
			{
				_isRemoveAdsBySubscription = true;
				RemoveAds(false);
			}
			else
			{
				_isRemoveAdsBySubscription = false;
			}
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			_stopAdv?.Dispose();
			_removeAdsIsBought?.Dispose();
		}
	}
	
	public interface IRemoveAdsModel
	{
		bool IsRemoveAds { get; }
		bool ConstantlyRemoveAds { get; }
	}
}