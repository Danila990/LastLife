using Core.Entity.Characters.Adapters;
using UnityEngine;

namespace Core.Player.MovementFSM.States.Interaction
{
	public class PlayerTeleportState : PlayerMovementState
	{
		public override bool CanExit => Fsm.Player.Constraints == AdapterConstraint.None;


		public PlayerTeleportState(PlayerMovementStateMachine fsm) : base(fsm)
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
			
			Fsm.Player.Rigidbody.velocity = Vector3.zero;
			Fsm.Player.Rigidbody.isKinematic = true;
			Fsm.Player.transform.position = Fsm.ReusableData.TeleportPosition;
			Fsm.Player.Rigidbody.position = Fsm.ReusableData.TeleportPosition;
			Fsm.Player.CurrentContext.PuppetMaster.enabled = false;
			//Fsm.Player.CurrentContext.PuppetMaster.Teleport(Fsm.ReusableData.TeleportPosition, Quaternion.identity, false);

		}

		public void ResetConstraints()
		{
			Fsm.Player.Constraints = AdapterConstraint.None;
		}

		public override void Exit()
		{
			base.Exit();
			Fsm.Player.Rigidbody.isKinematic = false;
			Fsm.Player.CurrentContext.PuppetMaster.enabled = true;
			Fsm.Player.Rigidbody.velocity = Vector3.zero;
		}
	}
}
