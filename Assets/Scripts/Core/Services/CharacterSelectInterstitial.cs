using System;
using Adv.Services.Interfaces;
using Core.InputSystem;
using MessagePipe;
using SharedUtils;
using UniRx;
using VContainer.Unity;

namespace Core.Services
{

	public class CharacterSelectInterstitial : IInitializable, IDisposable
	{
		private readonly ISubscriber<PlayerContextChangedMessage> _playerContextChangedSubscriber;
		private readonly IAdvService _advService;
		private const float INTERSTITIAL_INACTIVE_DURATION = 120f;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private bool _canShowAd = false;
		private IDisposable _timerDis;

		public CharacterSelectInterstitial(
			ISubscriber<PlayerContextChangedMessage> playerContextChangedSubscriber,
			IAdvService advService)
		{
			_playerContextChangedSubscriber = playerContextChangedSubscriber;
			_advService = advService;
		}
		
		public void Initialize()
		{
			_playerContextChangedSubscriber.Subscribe(OnPlayerContext).AddTo(_compositeDisposable);
			//_advService.OnShownRewardVideo.Subscribe(OnRewardShown).AddTo(_compositeDisposable);
			StartAdvTimer(INTERSTITIAL_INACTIVE_DURATION);
		}
		
		private void OnRewardShown(Unit obj)
		{
			StartAdvTimer(INTERSTITIAL_INACTIVE_DURATION);
		}

		private void OnPlayerContext(PlayerContextChangedMessage msg)
		{
			if (msg.Created && _canShowAd)
			{
				_advService.InterstitialRequest(null, "OnCharacterSelect");
				_canShowAd = false;
				StartAdvTimer(INTERSTITIAL_INACTIVE_DURATION);
			}
		}

		private void StartAdvTimer(float duration)
		{
			_timerDis?.Dispose();
			_timerDis = Observable
				.Timer(duration.ToSec(), Scheduler.MainThreadIgnoreTimeScale)
				.Subscribe(_ => _canShowAd = true);
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			_timerDis?.Dispose();
		}
	}
}