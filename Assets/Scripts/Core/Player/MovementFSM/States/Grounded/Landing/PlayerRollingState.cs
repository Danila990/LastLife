using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Landing
{
    public class PlayerRollingState : PlayerLandingState
    {
        public PlayerRollingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            //Fsm.ReusableData.MovementSpeedModifier = GroundedData.RollData.SpeedModifier;

            base.Enter();

            StartAnimation(AHash.RollParameterHash);

            Fsm.ReusableData.ShouldSprint = false;
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.RollParameterHash);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (Fsm.ReusableData.MovementInput != Vector2.zero)
            {
                return;
            }

            RotateTowardsTargetRotation();
        }
        
        public override void OnAnimationExitEvent()
        {
            if (Fsm.ReusableData.MovementInput == Vector2.zero)
            {
                Fsm.ChangeState(Fsm.MediumStoppingState);
            
                return;
            }
            OnMove();
        }
        
        public override void OnAnimationTriggerEvent(string key)
        {
            // if (Fsm.ReusableData.MovementInput == Vector2.zero)
            // {
            //     Fsm.ChangeState(Fsm.MediumStoppingState);
            //
            //     return;
            // }
            //
            // OnMove();
        }

        // protected override void OnJumpStarted(InputAction.CallbackContext context)
        // {
        // }
    }
}