using Utils.Constants;

namespace Core.Player.MovementFSM.Mech.States
{
    public class MechMovingState : MechGroundedState
    {
        public MechMovingState(MechMovementStateMachine fsm) : base(fsm)
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