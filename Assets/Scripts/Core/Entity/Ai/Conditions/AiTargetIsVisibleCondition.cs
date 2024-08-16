using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Core.Entity.Ai.Conditions
{
	[Name("Target out of range")]
	public class AiTargetIsVisibleCondition : ConditionTask<Transform>
	{
		public BBParameter<IAiTarget> AiTarget;
		public BBParameter<float> MaxSeenDistance;
		public BBParameter<bool> LoseTarget;
		
		protected override bool OnCheck()
		{
			if (Vector3.Distance(AiTarget.value.MovePoint, agent.position) > MaxSeenDistance.value)
			{
				LoseTarget.value = true;
				return true;
			}
			else
			{
				LoseTarget.value = false;
				return false;
			}
		}

		public override void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(agent.position, MaxSeenDistance.value);
		}
	}

}