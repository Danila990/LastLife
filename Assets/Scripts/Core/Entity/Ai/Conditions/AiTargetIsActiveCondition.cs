using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Core.Entity.Ai.Conditions
{
	[Category("Ai")]
	public class AiTargetIsActiveCondition : ConditionTask
	{
		public BBParameter<IAiTarget> AiTarget;

		protected override bool OnCheck()
		{
			var target = AiTarget.value;
			return target is { IsActive: true };
		}
	}

}