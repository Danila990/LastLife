using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace Core.Entity.Ai.Movement
{
	public class JumpingByTweenController : AiMovementController
	{
		[SerializeField] private NavMeshAgent _agent;
		
		[BoxGroup("Wander")] public float _keepDistance = 0.1f;
		[BoxGroup("Wander")] public float _minWanderDistance = 5;
		[BoxGroup("Wander")] public float _maxWanderDistance = 20;
		
		public override void Created(IObjectResolver resolver)
		{
			_agent.isStopped = true;
			_agent.updatePosition = false;
			_agent.updateRotation = false;
		}
		
		public override void SetDestination(Vector3 point)
		{
			_agent.SetDestination(point);
			AgentStatusType = AgentStatusType.MoveByPath;
		}

		private void Update()
		{
			// if (_agent.hasPath)
			// {
			// 	Debug.Log(_agent.path.corners);
			// }
			if (_agent.hasPath && _agent.desiredVelocity.magnitude > Mathf.Epsilon)
			{
				AgentStatusType = AgentStatusType.MoveByPath;
			}
			else
			{
				AgentStatusType = AgentStatusType.Waiting;
			}
		}
		
		public override void Disable()
		{
			_agent.isStopped = true;
			_agent.enabled = false;
		}
		
		public override void ResetPath()
		{
			if (_agent.enabled)
			{
				_agent.ResetPath();
			}
			AgentStatusType = AgentStatusType.Waiting;
		}
		
		public override void Wander()
		{
			var min = _minWanderDistance;
			var max = _maxWanderDistance;
			min = Mathf.Clamp(min, 0.01f, max);
			max = Mathf.Clamp(max, min, max);
			var wanderPos = transform.position;
			while ( ( wanderPos - transform.position ).magnitude < min ) {
				wanderPos = ( Random.insideUnitSphere * max ) + transform.position;
			}

			if ( NavMesh.SamplePosition(wanderPos, out var hit, _agent.height * 2, NavMesh.AllAreas) )
			{
				SetDestination(hit.position);
			}
		}
	}
}