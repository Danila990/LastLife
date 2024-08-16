using System;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UniRx;

namespace Core.Entity.Ai.Conditions
{
	[Category("Effect")]
	public class IsEffected : ConditionTask<AiCharacterAdapter>
	{
		public BBParameter<bool> Success;

		private IDisposable _handle;
		

		protected override bool OnCheck()
		{
			return agent.Effector.IsEffected == Success.value;
		}
	}
}
