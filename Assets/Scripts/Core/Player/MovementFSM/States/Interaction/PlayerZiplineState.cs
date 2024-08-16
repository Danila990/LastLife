using Core.Entity.Characters.Adapters;
using NodeCanvas.StateMachines;
using UnityEngine;

namespace Core.Player.MovementFSM.States.Interaction
{
	public class PlayerZiplineState : PlayerMovementState
	{

		public override bool CanExit => Fsm.Player.Constraints == AdapterConstraint.None || Fsm.ReusableData.IsDead;
		

		public PlayerZiplineState(PlayerMovementStateMachine fsm) : base(fsm)
		{
		}

		public override void Enter()
		{
			base.Enter();
			Fsm.Player.Constraints =
				AdapterConstraint.Falling
				| AdapterConstraint.Jumping
				| AdapterConstraint.Movement
				| AdapterConstraint.Rotation;
			Fsm.Player.CurrentContext.PuppetMaster.enabled = false;
		}

		public void ResetConstraints()
		{
			Fsm.Player.Constraints = AdapterConstraint.None;
		}

		public override void Exit()
		{
			base.Exit();
			Fsm.Player.CurrentContext.PuppetMaster.enabled = true;
			Fsm.Player.Rigidbody.velocity = Vector3.zero;
		}
	}

}
