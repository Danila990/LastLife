using System;
using SharedUtils;
using UniRx;

namespace Core.Timer
{
	public class SimpleTimer : Timer
	{
		
		private readonly TimeSpan _totalTime;
		private readonly ReactiveProperty<TimeSpan> _elapsedTime = new ReactiveProperty<TimeSpan>();
		private readonly ReactiveProperty<TimeSpan> _remainingTime = new ReactiveProperty<TimeSpan>();
		
		public override TimeSpan TotalTime => _totalTime;
		public override IReadOnlyReactiveProperty<TimeSpan> ElapsedTime => _elapsedTime;
		public override IReadOnlyReactiveProperty<TimeSpan> RemainingTime => _remainingTime;

		public SimpleTimer(TimeSpan totalTime)
		{
			_totalTime = totalTime;
			_elapsedTime.Value = 0f.ToSec();
			_remainingTime.Value = totalTime;
		}
		
		public SimpleTimer(TimeSpan totalTime, TimeSpan remainingTime)
		{
			_totalTime = totalTime;
			_elapsedTime.Value = totalTime - remainingTime;
			_remainingTime.Value = remainingTime;
		}
		
		protected override IDisposable StartTimerInternal(TimeSpan interval)
		{
			return Observable.Interval(interval)
				.Subscribe(_ =>
				{
					if (_elapsedTime.Value < _totalTime)
					{
						_elapsedTime.Value += OneSec;
						_remainingTime.Value = _totalTime - _elapsedTime.Value;
					}
					else
					{
						Dispose();
					}
				});
		}

		public void ChangeInterval(TimeSpan boostedInterval)
		{
			DisposeTimer();
			StartTimerWithInterval(boostedInterval);
		}
		
		public override void Dispose()
		{
			base.Dispose();
			_elapsedTime?.Dispose();
			_remainingTime?.Dispose();
		}
	}

	public abstract class Timer : ITimer, IDisposable
	{
		public readonly TimeSpan OneSec = 1f.ToSec();
		private readonly ReactiveCommand _onEnd = new ReactiveCommand();

		private IDisposable _timer;
		private bool _isEnded;
		
		public bool IsEnded => _isEnded;
		public abstract TimeSpan TotalTime { get; }
		public abstract IReadOnlyReactiveProperty<TimeSpan> ElapsedTime { get; }
		public abstract IReadOnlyReactiveProperty<TimeSpan> RemainingTime { get; }
		public IReactiveCommand<Unit> OnEnd => _onEnd;

		public void StartTimer()
		{
			if(_timer != null)
				return;

			_timer = StartTimerInternal(OneSec);
		}
		
		protected void StartTimerWithInterval(TimeSpan interval)
		{
			if(_timer != null)
				return;

			_timer = StartTimerInternal(interval);
		}
		
		protected abstract IDisposable StartTimerInternal(TimeSpan interval);

		protected void DisposeTimer()
		{
			_timer?.Dispose();
			_timer = null;
		}
		
		public virtual void Dispose()
		{
			DisposeTimer();
			_isEnded = true;
			_onEnd?.Execute();
			_onEnd?.Dispose();
		}
		
		public void DisposeWithoutEvent()
		{
			DisposeTimer();
			_isEnded = true;
			_onEnd?.Dispose();
		}
	}

	public interface ITimer
	{
		public bool IsEnded { get; }
		public TimeSpan TotalTime { get; }
		public IReadOnlyReactiveProperty<TimeSpan> ElapsedTime { get; }
		public IReadOnlyReactiveProperty<TimeSpan> RemainingTime { get; }
		public IReactiveCommand<Unit> OnEnd { get; }
	}
}
