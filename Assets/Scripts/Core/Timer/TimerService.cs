using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtils.PlayerPrefs;

namespace Core.Timer
{
	public interface ITimerService
	{
		public void AddTimer(string id, Timer timer);
		public bool TryGet(string id, out Timer timer);
		public bool TryRemove(string id, out Timer timer);
	}
	
	public class TimerService : ITimerService, IDisposable
	{

		private readonly Dictionary<string, Timer> _timers;

		public TimerService()
		{
			_timers = new Dictionary<string, Timer>();
		}

		public void AddTimer(string id, Timer timer)
		{
			_timers.TryAdd(id, timer);
		}

		public bool TryGet(string id, out Timer timer)
		{
			timer = null;
			_timers.TryGetValue(id, out timer);
			return timer != null;
		}
		
		public bool TryRemove(string id, out Timer timer)
		{
			timer = null;

			if (_timers.TryGetValue(id, out timer))
				_timers.Remove(id);
			
			return timer != null;
		}

		public void Dispose()
		{
			if(_timers.Count == 0)
				return;
			
			var timers = _timers.Values.ToArray();
			for (int i = 0; i < timers.Length; i++)
			{
				timers[i]?.DisposeWithoutEvent();
			}
		}
	}

}
