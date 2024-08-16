namespace Core.Player.MovementFSM.States.Grounded.Stopping
{
    public class PlayerLightStoppingState : PlayerStoppingState
    {
        public PlayerLightStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Fsm.ReusableData.MovementDecelerationForce = GroundedData.StopData.LightDecelerationForce;

            Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.WeakForce;
        }
    }
}