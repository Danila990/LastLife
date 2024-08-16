using System;
using SharedUtils;
using UnityEngine;

namespace Core.Timer
{

	public interface ITimerProvider
	{
		public ITimer AddOrGetTimer(string id, TimeSpan durationInSec);
		public ITimer AddOrGetTimer(string id, TimeSpan duration, TimeSpan remainingTime);
		public bool GetIfExist(string id, out Timer existedTimer);

		public void RemoveTimer(string id);
	}

	public class TimerProvider : ITimerProvider
	{
		private readonly ITimerService _timerService;

		public TimerProvider(ITimerService timerService)
		{
			_timerService = timerService;
		}

		public ITimer AddOrGetTimer(string id, TimeSpan duration)
			=>  GetIfExist(id, out var existedTimer)
				? existedTimer 
				: AddTimer(id, new SimpleTimer(duration));

		public ITimer AddOrGetTimer(string id, TimeSpan duration, TimeSpan remainingTime) 
			=>  GetIfExist(id, out var existedTimer)
				? existedTimer
				: AddTimer(id, new SimpleTimer(duration, remainingTime));

		public void RemoveTimer(string id)
		{
			if (_timerService.TryRemove(id, out var timer))
			{
				timer.Dispose();
			}
		}
		
		private ITimer AddTimer(string id, Timer timer)
		{
			_timerService.AddTimer(id, timer);
			timer.StartTimer();
			return timer;
		}

		public bool GetIfExist(string id, out Timer existedTimer)
			=> _timerService.TryGet(id, out existedTimer);
	}
}
