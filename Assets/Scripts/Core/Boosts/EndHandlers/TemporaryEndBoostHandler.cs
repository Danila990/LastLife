using System;
using Core.Boosts.Impl;
using Core.Timer;
using SharedUtils;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Boosts.EndHandlers
{
	public class TemporaryEndBoostHandler : EndBoostHandler
	{
		[Inject] private readonly ITimerProvider _timerProvider;

		private IDisposable _disposable;
		
		public ITimer Timer { get; private set; }
		
		public TemporaryEndBoostHandler(IBoostImpl boostImpl) : base(boostImpl) { }

		protected override void OnBoostApplied()
		{
			if(_timerProvider.GetIfExist(Id, out _))
				return;
			
			Timer = _timerProvider.AddOrGetTimer(Id, BoostImpl.BoostArgs.Duration.ToSec());
			
			_disposable?.Dispose();
			_disposable = Timer.OnEnd.Subscribe(_ => Dispose());
		}

		protected override void EndBoostInternal()
		{
			BoostImpl.RemoveEffect();
		}
		
		public override void Dispose()
		{
			_disposable?.Dispose();
			base.Dispose();
			_timerProvider.RemoveTimer(Id);
		}
	}

}
