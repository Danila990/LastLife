using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.Mech.States
{
    public class MechFallingState : MechAirborneState
    {
        private Vector3 _playerPositionOnEnter;
        
        public MechFallingState(MechMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            StartAnimation(AHash.FallParameterHash);

            _playerPositionOnEnter = Fsm.Player.transform.position;
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.FallParameterHash);
        }

        public override void Update()
        {
            base.Update();

            EnterHardFalling();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            CheckGround();
            LimitVerticalVelocity();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            if(Fsm.Player.Rigidbody.velocity.y < FALLING_THRESHOLD)
                Fsm.ReusableData.VerticalVelocity = GetPlayerVerticalVelocity();
        }

        private void LimitVerticalVelocity()
        {
            var playerVerticalVelocity = GetPlayerVerticalVelocity();
            Fsm.Player.StatsProvider.Stats.GetValue(StatType.FallSpeedLimit, out var stat);
            if (playerVerticalVelocity.y >= -AirborneData.FallData.FallSpeedLimit + stat)
            {
                return;
            }

            var limitY = -AirborneData.FallData.FallSpeedLimit - playerVerticalVelocity.y + stat;
            var limitedVelocityForce = new Vector3(0f, limitY, 0f);
            
            Fsm.Player.Rigidbody.AddForce(limitedVelocityForce, ForceMode.VelocityChange);
        }

        protected override void ResetSprintState()
        {
            
        }
        
        protected virtual void EnterHardFalling()
        {		
            if (Fsm.HasConstraint(AdapterConstraint.Falling))
                return;
        }
        
        protected override void ExitFalling()
        {
            var fallDistance = _playerPositionOnEnter.y - Fsm.Player.transform.position.y;

            if (Fsm.HasConstraint(AdapterConstraint.Falling))
            {
                Fsm.ChangeState(Fsm.IdlingState);
                return;
            }
            Fsm.ChangeState(Fsm.LandingState);
        }
    }
}