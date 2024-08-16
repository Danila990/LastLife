using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Landing
{
    public class PlayerLandingState : PlayerGroundedState
    {
        public PlayerLandingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Fsm.ReusableData.VerticalVelocity = Vector3.zero;

            StartAnimation(AHash.LandingParameterHash);
            //
            // DisableCameraRecentering();
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.LandingParameterHash);
        }
    }
}