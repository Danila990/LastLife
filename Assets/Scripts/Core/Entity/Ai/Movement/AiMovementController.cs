using UnityEngine;
using VContainer;

namespace Core.Entity.Ai.Movement
{
	public abstract class AiMovementController : MonoBehaviour
	{
		[field:SerializeField] public float StoppingDistance { get; set; }
		[field:SerializeField] public float Speed { get; set; }
		[field:SerializeField] public float TurnSpeed { get; set; }
		
		[field:SerializeField] public bool IsStopped { get; set; }

		public AgentStatusType AgentStatusType;
		public abstract void SetDestination(Vector3 point);
		public abstract void Created(IObjectResolver resolver);
		public abstract void Disable();
		public abstract void ResetPath();
		public abstract void Wander();
	}

	public enum AgentStatusType
	{
		Waiting,
		MoveByPath,
	}
}