using Core.Entity.Characters.Adapters;
using UnityEngine;

namespace Core.Player.MovementFSM.States.Grounded.Moving
{
	public class PlayerManualWalk : PlayerWalkingState
	{
		public override bool CanExit => Fsm.Player.Constraints == AdapterConstraint.None || Fsm.ReusableData.IsDead;

		public PlayerManualWalk(PlayerMovementStateMachine fsm) : base(fsm)
		{
		}
		
		public override void HandleInput()
		{
			
		}
		
		public override void PhysicsUpdate()
		{
			Float();
			var movementDirection = GetMovementInputDirection();
			var targetRotationYAngle = UpdateTargetRotation(movementDirection, false);
			var targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);
			var movementSpeed = GetMovementSpeed();
			var currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
			Fsm.Player.Rigidbody.AddForce(targetRotationDirection * movementSpeed - currentPlayerHorizontalVelocity, ForceMode.VelocityChange);
		}
	}
}