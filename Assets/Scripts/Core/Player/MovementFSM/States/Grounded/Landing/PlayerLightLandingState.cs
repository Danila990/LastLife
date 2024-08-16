using UnityEngine;

namespace Core.Player.MovementFSM.States.Grounded.Landing
{
    public class PlayerLightLandingState : PlayerLandingState
    {
        public PlayerLightLandingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            Fsm.ReusableData.MovementSpeedModifier = 0;

            base.Enter();

            Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.StationaryForce;

            ResetVelocity();
        }

        public override void Update()
        {
            base.Update();

            if (Fsm.ReusableData.MovementInput == Vector2.zero)
            {
                return;
            }

            OnMove();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (!IsMovingHorizontally())
            {
                return;
            }

            ResetVelocity();
        }
        
        public override void OnAnimationTriggerEvent(string key)
        {
            Fsm.ChangeState(Fsm.IdlingState);
        }
        
        public override void OnAnimationExitEvent()
        {
            Fsm.ChangeState(Fsm.IdlingState);
        }
    }
}