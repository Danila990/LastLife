namespace Core.Player.MovementFSM.States.Grounded.Stopping
{
    public class PlayerHardStoppingState : PlayerStoppingState
    {
        public PlayerHardStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            //StartAnimation(stateMachine.Player.AnimationData.HardStopParameterHash);

            Fsm.ReusableData.MovementDecelerationForce = GroundedData.StopData.HardDecelerationForce;
            Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.StrongForce;
        }

        public override void Exit()
        {
            base.Exit();

            //StopAnimation(stateMachine.Player.AnimationData.HardStopParameterHash);
        }

        protected override void OnMove()
        {
            if (Fsm.ReusableData.ShouldWalk)
            {
                return;
            }

            Fsm.ChangeState(Fsm.RunningState);
        }
    }
}