using System;
using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.Mech.States
{
    public class MechStationaryAttackState : MechGroundAction
    {
        public Action Callback { get; set; }
		
        public MechStationaryAttackState(MechMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
			
        }

        public override void Enter()
        {
            Fsm.ReusableData.MovementSpeedModifier = 0;
            Fsm.ReusableData.CurrentJumpForce = Vector3.zero;

            base.Enter();
            StartAnimation(AHash.IsMeleeAttack);
            ResetVelocity();
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

        public override void Exit()
        {
            base.Exit();
            StopAnimation(AHash.IsMeleeAttack);
        }
		
        protected override void OnMove()
        {
			
        }

        protected override void OnJumpStarted()
        {
			
        }

        public override void OnAnimationTriggerEvent(string key)
        {
            switch (key)
            {
                case "Exit":
                    Fsm.ChangeState(Fsm.IdlingState);
                    break;
                case "Action":
                    Callback?.Invoke();
                    Callback = null;
                    break;
            }
        }
    }
}