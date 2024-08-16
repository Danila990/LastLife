using System;
using System.Collections.Generic;
using Core.Timer;
using SharedUtils;
using UniRx;

namespace Market.OilRig
{
	public interface IRefinerBoostService
	{
		public bool Boost(string refinerId, float interval, out ITimer boosterTimer);
	}
	
	public class RefinerBoostService : IRefinerBoostService
	{
		private readonly ITimerProvider _timerProvider;

		private readonly Dictionary<string, (TimeSpan Duration, TimeSpan Interval)> _boosted;

		public RefinerBoostService(ITimerProvider timerProvider)
		{
			_timerProvider = timerProvider;
			_boosted = new();
		}

		public bool Boost(string refinerId, float interval, out ITimer boosterTimer)
		{
			boosterTimer = null;
			if(_timerProvider.GetIfExist(refinerId, out var tm) && tm is SimpleTimer timer)
			{
				boosterTimer = timer;
				timer.ChangeInterval(interval.ToSec());
			}

			return boosterTimer != null;
		}


	}
}
