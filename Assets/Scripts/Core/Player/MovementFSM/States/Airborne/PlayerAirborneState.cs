using UnityEngine;
using Utils;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Airborne
{
    public class PlayerAirborneState : PlayerMovementState
    {

        protected const float FALLING_THRESHOLD = -2f;
        public PlayerAirborneState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Fsm.ReusableData.InAir.Value = true;
            AddInputListeners();
            StartAnimation(AHash.AirborneParameterHash);
            ResetSprintState();
        }

        public override void Exit()
        {
            base.Exit();
            RemoveInputListeners();
            StopAnimation(AHash.AirborneParameterHash);
        }

        private void AddInputListeners()
        {
            Fsm.ReusableData.OnJumpDown += OnJumpDown;
        }

        private void RemoveInputListeners()
        {
            Fsm.ReusableData.OnJumpDown -= OnJumpDown;
        }

        private void OnJumpDown()
        {
            Fsm.ChangeState(Fsm.JetPackState);
        }
        
        protected void CheckGround()
        {
            var colliderData = Fsm.Player.CapsuleColliderUtility.TriggerColliderData;
        

            var grounded = Physics.CheckBox(
                colliderData.GroundCheckCollider.bounds.center,
                colliderData.GroundCheckCollider.bounds.extents / 2f,
                Quaternion.identity,
                LayerMasks.WalkableMask,
                QueryTriggerInteraction.Ignore);

            if (grounded)
            {
                ExitFalling();
            }
        }
        
        protected virtual void ResetSprintState()
        {
            Fsm.ReusableData.ShouldSprint = false;
        }

        protected override void OnContactWithGround(Collider collider)
        {
            ExitFalling();
        }

        protected virtual void ExitFalling()
        {
            Fsm.ChangeState(Fsm.LightLandingState);
        }
    }
}