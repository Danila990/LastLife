using System;
using Adv.Services;
using Adv.Services.Interfaces;
using Core.Pause;
using SharedUtils;
using UniRx;
using VContainer.Unity;

namespace Core.Services
{
	public class InterstitialByTimer : IInitializable, IDisposable
	{
		private readonly IAdvService _advService;
		private readonly IPauseService _pauseService;
		private readonly IAdvSettings _advSettings;
		private IDisposable _timerDis;
		private float _currentTimer = 0;
		
		public InterstitialByTimer(
			IAdvService advService,
			IPauseService pauseService,
			IAdvSettings advSettings)
		{
			_advService = advService;
			_pauseService = pauseService;
			_advSettings = advSettings;
		}
		
		public void Initialize()
		{
			StartAdvTimer(_advSettings.InterTimer);
		}
		
		private void StartAdvTimer(float duration)
		{
			_currentTimer = duration;
			_timerDis?.Dispose();
			_timerDis = Observable
				.Interval(1f.ToSec(), Scheduler.MainThreadIgnoreTimeScale)
				.Subscribe(ShowInter);
		}
		
		private void ShowInter(long obj)
		{
			if (_pauseService.IsPaused)
				return;
			
			_currentTimer -= 1f;
			if (_currentTimer <= 0)
			{
				_timerDis?.Dispose();
				_advService.InterstitialRequestWithWindow(null, "InterByTimer");
				StartAdvTimer(_advSettings.InterTimer);
			}
		}

		private void OnClose()
		{
			StartAdvTimer(_advSettings.InterTimer);
		}

		public void Dispose()
		{
			_timerDis?.Dispose();
		}
	}
}