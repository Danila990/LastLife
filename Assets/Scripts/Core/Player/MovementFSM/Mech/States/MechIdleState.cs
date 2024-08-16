using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.Mech.States
{
    public class MechIdleState : MechGroundedState
    {
        public MechIdleState(MechMovementStateMachine fsm) : base(fsm)
        {
        }
		
        #region IState
        public override void Enter()
        {
            Fsm.ReusableData.MovementSpeedModifier = 0f;

            //Fsm.ReusableData.BackwardsCameraRecenteringData = GroundedData.IdleData.BackwardsCameraRecenteringData;
            base.Enter();

            StartAnimation(AHash.IdleParameterHash);
            Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.StationaryForce;
            ResetVelocity();
        }
		
        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.IdleParameterHash);
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

            if (!IsMovingHorizontally(0))
            {
                return;
            }

            ResetVelocity();
        }
        #endregion
    }
}