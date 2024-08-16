using Core.Entity.Characters.Adapters;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;
using Utils.Constants;

namespace Core.Entity.Ai.AiActions
{
	[Name("Chase AiTarget")]
	[Category("Movement/Pathfinding")]
	public class ChaseAiTarget : ActionTask<AiCharacterAdapter>
	{
		[RequiredField]
		public BBParameter<IAiTarget> target;
		public BBParameter<float> speed = 4;
		public BBParameter<float> keepDistance = 0.1f;
		
		private Vector3 lastRequest;

		protected override string info {
			get { return "Chase " + target; }
		}
		
		protected override void OnExecute() {
			if (target.value == null)
			{
				EndAction(false);
				return;
			}
			agent.NavMeshAgent.speed = speed.value;
		}
		
		protected override void OnUpdate() {
			if (target.value is not { IsActive: true } || !agent.enabled)
			{
				EndAction(false); return;
			}
			
			var pos = target.value.MovePoint;
			if ( lastRequest != pos ) {
				if ( !agent.NavMeshAgent.SetDestination(pos) ) {
					EndAction(false);
					return;
				}
			}

			lastRequest = pos;

			float sqrMagnitude = (target.value.MovePoint - agent.NavMeshAgent.transform.position).sqrMagnitude;
			float sqrLength = Mathf.Pow(agent.NavMeshAgent.stoppingDistance, 2) + Mathf.Pow(keepDistance.value, 2);

			if (sqrMagnitude <= sqrLength)
				agent.NavMeshAgent.SetDestination(agent.transform.position);
			
			agent.CurrentContext.CharacterAnimator.Animator.SetBool(AHash.SprintParameterHash, agent.NavMeshAgent.velocity.sqrMagnitude < 25f);
		}

		protected override void OnPause()
		{
			OnStop();
		}
		
		protected override void OnStop() {
			
			if ( agent.enabled && agent.NavMeshAgent.isOnNavMesh)
				agent.NavMeshAgent.ResetPath();
			
			lastRequest = Vector3.zero;
			
			agent.CurrentContext.CharacterAnimator.Animator.SetBool(AHash.SprintParameterHash, false);
		}
	}
}
