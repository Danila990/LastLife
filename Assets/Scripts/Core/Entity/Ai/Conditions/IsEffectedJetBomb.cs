using System;
using Core.Effects;
using Core.Entity.Head;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Core.Entity.Ai.Conditions
{
	[Category("Effect")]
	public class IsEffectedJetBomb : ConditionTask<JetBombContext>
	{
		public BBParameter<bool> Success;

		private IDisposable _handle;
		

		protected override bool OnCheck()
		{
			return agent.Effector.IsEffected == Success.value;
		}
	}
	
	[Category("Effect")]
	public class IsEffectedGeneric : ConditionTask<SimpleEffector>
	{
		public BBParameter<bool> Success;

		private IDisposable _handle;
		

		protected override bool OnCheck()
		{
			return agent.IsEffected == Success.value;
		}
	}
}
