using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Stopping
{
    public class PlayerStoppingState : PlayerGroundedState
    {
        public PlayerStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            Fsm.ReusableData.MovementSpeedModifier = 0f;

            //SetBaseCameraRecenteringData();

            base.Enter();

            StartAnimation(AHash.StoppingParameterHash);
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.StoppingParameterHash);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            RotateTowardsTargetRotation();

            if (!IsMovingHorizontally())
            {
                return;
            }

            DecelerateHorizontally();
        }

        public override void OnAnimationTriggerEvent(string key)
        {
            Fsm.ChangeState(Fsm.IdlingState);
        }

        protected override void AddInputActionsCallbacks()
        {
            base.AddInputActionsCallbacks();

            //Fsm.Player.Input.PlayerActions.Movement.started += OnMovementStarted;
        }

        protected override void RemoveInputActionsCallbacks()
        {
            base.RemoveInputActionsCallbacks();

            //Fsm.Player.Input.PlayerActions.Movement.started -= OnMovementStarted;
        }

        // private void OnMovementStarted(InputAction.CallbackContext context)
        // {
        //     OnMove();
        // }
    }
}