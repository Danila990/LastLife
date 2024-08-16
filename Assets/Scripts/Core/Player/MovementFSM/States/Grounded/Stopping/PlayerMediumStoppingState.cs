namespace Core.Player.MovementFSM.States.Grounded.Stopping
{
    public class PlayerMediumStoppingState : PlayerStoppingState
    {
        public PlayerMediumStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            //StartAnimation(stateMachine.Player.AnimationData.MediumStopParameterHash);

            Fsm.ReusableData.MovementDecelerationForce = GroundedData.StopData.MediumDecelerationForce;

            Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.MediumForce;
        }

        public override void Exit()
        {
            base.Exit();

            //StopAnimation(stateMachine.Player.AnimationData.MediumStopParameterHash);
        }
    }
}