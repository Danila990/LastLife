using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Moving
{
    public class PlayerMovingState : PlayerGroundedState
    {
        public PlayerMovingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

           StartAnimation(AHash.MovingParameterHash);
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.MovingParameterHash);
        }
    }
}