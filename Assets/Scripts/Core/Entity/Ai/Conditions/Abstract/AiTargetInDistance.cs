using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace Core.Entity.Ai.Conditions.Abstract
{
	[Category("Ai")]
	public abstract class AiTargetInDistance : ConditionTask<Transform>
	{
		[RequiredField]
		public BBParameter<IAiTarget> AiTarget;
		public CompareMethod checkType = CompareMethod.LessThan;
		public abstract float Distance { get; }

		[SliderField(0, 0.1f)]
		public float floatingPoint = 0.05f;

		// protected override string info {
		// 	get { return "Distance" + OperationTools.GetCompareString(checkType) + Distance + " to " + AiTarget.value; }
		// }

		protected override bool OnCheck() {
			return OperationTools.Compare(Vector3.Distance(agent.position, AiTarget.value.MovePoint), Distance, checkType, floatingPoint);
		}
		
	}
}