using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SharedUtils.PlayerPrefs;
using VContainer.Unity;

namespace InAppReview
{
	public class IAPReview : IAsyncStartable
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		const string IAP_REVIEW_KEY = "iap_review";
		
		public IAPReview(IPlayerPrefsManager playerPrefsManager)
		{
			_playerPrefsManager = playerPrefsManager;
		}
		
		public async UniTask StartAsync(CancellationToken cancellation)
		{
			if (_playerPrefsManager.GetValue<bool>(IAP_REVIEW_KEY, false))
				return;
			
			await UniTask.Delay(TimeSpan.FromMinutes(2), cancellationToken: cancellation);
			IInAppReview inAppReview = null;
#if UNITY_IOS
			inAppReview = new IosInAppReview();
#elif UNITY_ANDROID
			inAppReview = new AndroidInAppReview();
#endif
			inAppReview?.RequestInAppReview(cancellation);

			_playerPrefsManager.SetValue(IAP_REVIEW_KEY, true);
		}
	}

}