using Core.Entity.Ai.Movement;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Core.Entity.Ai.AiActions
{
	[Category("Movement/Pathfinding")]
	public class CustomSeek : ActionTask<AiMovementController>
	{
		[RequiredField]
		public BBParameter<IAiTarget> target;

		private Vector3 lastRequest;

		protected override string info {
			get { return "Seek " + target; }
		}
		
		protected override void OnExecute() {
			if (target.value == null)
			{
				EndAction(false); return;
			}
			if ( Vector3.Distance(agent.transform.position, target.value.MovePoint) <= agent.StoppingDistance) {
				EndAction(true);
			}
		}
		
		protected override void OnUpdate() {
			if (target.value is not { IsActive: true } || !agent.enabled)
			{
				EndAction(false); return;
			}
			var pos = target.value.MovePoint;
			if ( lastRequest != pos ) {
				agent.SetDestination(pos);
				if (agent.AgentStatusType == AgentStatusType.Waiting)
				{
					EndAction(true);
				}
			}

			lastRequest = pos;

			if ( agent.AgentStatusType == AgentStatusType.Waiting) {
				EndAction(true);
			}
		}

		protected override void OnPause()
		{
			OnStop();
		}
		
		protected override void OnStop() {
			if ( agent.enabled ) {
				agent.ResetPath();
			}
			lastRequest = Vector3.zero;
		}
	}
}