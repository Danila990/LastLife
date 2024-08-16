using System;
using Core.Boosts.Impl;
using Core.Entity.Characters;
using UniRx;

namespace Core.Boosts.EndHandlers
{
	public class ContextDeathEndBoostHandler : EndBoostHandler
	{
		private readonly CharacterContext _context;
		private IDisposable _disposable;
		
		public ContextDeathEndBoostHandler(IBoostImpl boostImpl, CharacterContext context) : base(boostImpl) {
			_context = context;
		}

		protected override void OnBoostApplied()
		{
			_disposable?.Dispose();
			_disposable = _context.Health.OnDeath.Subscribe(_ => Dispose());
		}

		protected override void EndBoostInternal()
		{
			BoostImpl.RemoveEffect();
		}
		
		public override void Dispose()
		{
			base.Dispose();
			_disposable?.Dispose();
		}
	}
}
