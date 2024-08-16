using System;
using Core.Boosts.Impl;
using UniRx;
using UnityEngine;

namespace Core.Boosts.EndHandlers
{
	public abstract class EndBoostHandler : IEndBoostHandler
	{
		protected readonly IBoostImpl BoostImpl;
		private readonly ReactiveCommand<string> _onBoostEnded;
		private readonly IDisposable _disposable;

		public string Id => $"{BoostImpl.AdditionalName}_{BoostImpl.BoostArgs.Type}";
		public string Type => BoostImpl.BoostArgs.Type;
		public IReactiveCommand<string> OnBoostEnded => _onBoostEnded;

		protected EndBoostHandler(IBoostImpl boostImpl)
		{
			BoostImpl = boostImpl;
			_onBoostEnded = new ReactiveCommand<string>();
			_disposable = boostImpl.OnApply.Subscribe(_ => OnBoostApplied());
		}
		
		protected abstract void EndBoostInternal();
		protected abstract void OnBoostApplied();
		
		public virtual void Dispose()
		{
			EndBoostInternal();
			_onBoostEnded?.Execute(BoostImpl.BoostArgs.Type);
			_onBoostEnded?.Dispose();
			_disposable?.Dispose();
		}
	}
}
