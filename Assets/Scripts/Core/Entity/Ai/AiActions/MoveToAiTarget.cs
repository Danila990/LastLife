using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Entity.Ai.AiActions
{
	[Name("Seek AiTarget")]
	[Category("Movement/Pathfinding")]
	public class MoveToAiTarget : ActionTask<NavMeshAgent>
	{
		[RequiredField]
		public BBParameter<IAiTarget> target;
		public BBParameter<float> speed = 4;
		public BBParameter<float> keepDistance = 0.1f;
		
		private Vector3 lastRequest;

		protected override string info {
			get { return "Seek " + target; }
		}
		
		protected override void OnExecute() {
			if (target.value == null)
			{
				EndAction(false); return;
			}
			agent.speed = speed.value;
			if ( Vector3.Distance(agent.transform.position, target.value.MovePoint) <= agent.stoppingDistance + keepDistance.value ) {
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
				if ( !agent.SetDestination(pos) ) {
					EndAction(false);
					return;
				}
			}

			lastRequest = pos;

			if ( !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance.value ) {
				EndAction(true);
			}
		}

		protected override void OnPause()
		{
			OnStop();
		}
		
		protected override void OnStop() {
			if ( agent.enabled && agent.isOnNavMesh) {
				agent.ResetPath();
			}
			lastRequest = Vector3.zero;
		}
		
		public override void OnDrawGizmosSelected() {
			if ( target.value != null ) {
				Gizmos.DrawWireSphere(target.value.MovePoint, keepDistance.value);
			}
		}
	}
}