using System;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using NodeCanvas.Framework;
using UniRx;

namespace Core.Entity.Ai.AiActions
{
	public class AiOnGetDamage : ActionTask<AiCharacterAdapter>
	{
		public BBParameter<DamageType> Type;
		
		private IDisposable _disposable;
		
		protected override void OnExecute()
		{
			_disposable = agent.CurrentContext.Health.OnDamage.Subscribe(OnGetDamage);
		}

		private void OnGetDamage(DamageArgs args)
		{
			if(args.DamageType == Type.value)
				EndAction(true);
		}

		protected override void OnStop()
		{
			_disposable?.Dispose();
		}
	}
}
