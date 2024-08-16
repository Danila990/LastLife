using System;
using Core.Entity.Characters;
using SharedUtils;
using UniRx;
using Utils;

namespace Core.Effects
{
	public class Effect : IDisposable
	{
		public readonly EffectType EffectType;
		
		private readonly Action _start;
		private readonly Action _end;
		private readonly float _duration;

		private IDisposable _effect;
		private bool _isDisposed;

		public Effect(Action start, Action end, float duration, EffectType effectType)
		{
			_start = start;
			_end = end;
			_duration = duration;
			EffectType = effectType;
		}

		public void Start()
		{
			_start.Invoke();
			Renew();
		}

		public void Renew()
		{
			_effect?.Dispose();
			_effect = Observable
				.Timer(_duration.ToSec())
				.Subscribe(_ => Finally());
		}
		
		private void Finally()
		{
			if (_isDisposed)
				return;
			
			_end.Invoke();
		}

		public void ForceStop()
		{
			_end.Invoke();
		}
		
		public void Dispose()
		{
			_isDisposed = true;
			_effect?.Dispose();
		}
	}
}
