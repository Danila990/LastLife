using System;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Core.Entity.Ai.Conditions
{
	[Category("Effect")]
	public class IsHeadEffected : ConditionTask<BaseHeadAdapter>
	{
		public BBParameter<bool> Success;

		private IDisposable _handle;
		

		protected override bool OnCheck()
		{
			return agent.Effector.IsEffected == Success.value;
		}
	}
}
