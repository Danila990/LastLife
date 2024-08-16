using System;
using Core.Boosts.Impl;
using Core.Entity.Characters;
using UniRx;

namespace Core.Boosts.EndHandlers
{
	public class ContextDestroyEndBoostHandler : EndBoostHandler
	{
		private readonly CharacterContext _context;
		private IDisposable _disposable;
		
		public ContextDestroyEndBoostHandler(IBoostImpl boostImpl, CharacterContext context) : base(boostImpl) {
			_context = context;
		}

		protected override void OnBoostApplied()
		{
			_disposable?.Dispose();
			_disposable = _context.OnDestroyCommand.Subscribe(_ => Dispose());
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
