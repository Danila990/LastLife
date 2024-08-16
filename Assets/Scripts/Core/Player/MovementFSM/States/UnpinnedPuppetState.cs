using Core.Player.MovementFSM.States.Grounded;
using RootMotion.Dynamics;
using UnityEngine;

namespace Core.Player.MovementFSM.States
{
	public class UnpinnedPuppetState : PlayerGroundedState
	{
		public UnpinnedPuppetState(PlayerMovementStateMachine fsm) : base(fsm)
		{
			
		}

		public override void Enter()
		{
			base.Enter();
			
			Fsm.ReusableData.MovementSpeedModifier = 0;
			Fsm.ReusableData.CurrentJumpForce = Vector3.zero;

			if (Fsm.Player.CurrentContext.BehaviourPuppet.state != BehaviourPuppet.State.Unpinned)
			{
				Fsm.Player.CurrentContext.BehaviourPuppet.SetState(BehaviourPuppet.State.Unpinned);
			}
		}

		public override void OnLoseBalance()
		{
			
		}
	}
}