using NodeCanvas.Framework;
using UnityEngine;

namespace Core.Entity.Ai.Conditions
{
	public class SaveLastSeenPoint : ActionTask
	{
		public BBParameter<IAiTarget> AiTarget;
		public BBParameter<Vector3> LastSeen;

		protected override void OnExecute()
		{
			LastSeen.value = AiTarget.value.MovePoint;
			EndAction();
		}
	}
}