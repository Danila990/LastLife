using UnityEngine;
using Utils;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded
{
    public class PlayerGroundedState : PlayerMovementState
    {
        public PlayerGroundedState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Fsm.ReusableData.InAir.Value = false;
            StartAnimation(AHash.GroundedParameterHash);

            UpdateShouldSprintState();

            //UpdateCameraRecenteringState(Fsm.ReusableData.MovementInput);
        }

        public override void Exit()
        {
            base.Exit();
                
            StopAnimation(AHash.GroundedParameterHash);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Float();
        }

        protected virtual void OnUseAction()
        {
            Fsm.ChangeState(Fsm.StationaryAttackState);
        }

        private void UpdateShouldSprintState()
        {
            if (!Fsm.ReusableData.ShouldSprint)
            {
                return;
            }

            if (Fsm.ReusableData.MovementInput != Vector2.zero)
            {
                return;
            }

            Fsm.ReusableData.ShouldSprint = false;
        }

        protected void Float()
        {
            var capsuleColliderCenterInWorldSpace = Fsm.Player.CapsuleColliderUtility.CapsuleColliderData.Collider.bounds.center;
        
            var downwardsRayFromCapsuleCenter = new Ray(capsuleColliderCenterInWorldSpace, Vector3.down);
        
            if (Physics.Raycast(downwardsRayFromCapsuleCenter, out var hit, Fsm.Player.CapsuleColliderUtility.SlopeData.FloatRayDistance, LayerMasks.WalkableMask, QueryTriggerInteraction.Ignore))
            {
                var groundAngle = Vector3.Angle(hit.normal, -downwardsRayFromCapsuleCenter.direction);
        
                var slopeSpeedModifier = SetSlopeSpeedModifierOnAngle(groundAngle);
        
                if (slopeSpeedModifier == 0f)
                {
                    return;
                }
        
                var distanceToFloatingPoint = Fsm.Player.CapsuleColliderUtility.CapsuleColliderData.ColliderCenterInLocalSpace.y * Fsm.Player.transform.localScale.y - hit.distance;
        
                if (distanceToFloatingPoint == 0f)
                {
                    return;
                }
        
                var amountToLift = distanceToFloatingPoint * Fsm.Player.CapsuleColliderUtility.SlopeData.StepReachForce - GetPlayerVerticalVelocity().y;
        
                var liftForce = new Vector3(0f, amountToLift, 0f);
        
                Fsm.Player.Rigidbody.AddForce(liftForce, ForceMode.VelocityChange);
            }
        }
        
        private float SetSlopeSpeedModifierOnAngle(float angle)
        {
            float slopeSpeedModifier = GroundedData.SlopeSpeedAngles.Evaluate(angle);
        
            if (Fsm.ReusableData.MovementOnSlopesSpeedModifier != slopeSpeedModifier)
            {
                Fsm.ReusableData.MovementOnSlopesSpeedModifier = slopeSpeedModifier;
            }
        
            return slopeSpeedModifier;
        }

        protected override void AddInputActionsCallbacks()
        { 
            base.AddInputActionsCallbacks();
            
            Fsm.ReusableData.OnJumpDown += OnJumpStarted;
        }
        
        protected override void RemoveInputActionsCallbacks()
        {
            base.RemoveInputActionsCallbacks();
            
            Fsm.ReusableData.OnJumpDown -= OnJumpStarted;
        }
        
        protected virtual void OnJumpStarted()
        {
            Fsm.ChangeState(Fsm.JumpingState);
        }

        protected virtual void OnMove()
        {
            if (Fsm.ReusableData.CarryInventory && Fsm.ReusableData.CarryInventory.HasContext)
            {
                Fsm.ChangeState(Fsm.CarryingWalkState);
                return;
            }
            
            if (Fsm.ReusableData.IsAiming)
            {
                Fsm.ChangeState(Fsm.WalkingState);
                return;
            }
            
            if (Fsm.ReusableData.ShouldSprint)
            {
                Fsm.ChangeState(Fsm.SprintingState);

                return;
            }

            Fsm.ChangeState(Fsm.RunningState);
        }

        protected override void OnContactWithGroundExited(Collider collider)
        {
            if (IsThereGroundUnderneath())
            {
                return;
            }
        
            var capsuleColliderCenterInWorldSpace = Fsm.Player.CapsuleColliderUtility.CapsuleColliderData.Collider.bounds.center;
            var downwardsRayFromCapsuleBottom = new Ray(capsuleColliderCenterInWorldSpace - Fsm.Player.CapsuleColliderUtility.CapsuleColliderData.ColliderVerticalExtents, Vector3.down);
        
            if (!Physics.Raycast(downwardsRayFromCapsuleBottom, out _, GroundedData.GroundToFallRayDistance, LayerMasks.WalkableMask, QueryTriggerInteraction.Ignore)) //TODO: USE SPHERE NO RAYCAST OR USE TIMEOUT(PREFERED) OR USE ANIMATION EVENT
            {
                OnFall();
            }
        }
        
        private bool IsThereGroundUnderneath()
        {
            var triggerColliderData = Fsm.Player.CapsuleColliderUtility.TriggerColliderData;
        
            var groundColliderCenterInWorldSpace = triggerColliderData.GroundCheckCollider.bounds.center;
            
            var checkBox = Physics.CheckBox(
                groundColliderCenterInWorldSpace, 
                triggerColliderData.GroundCheckColliderVerticalExtents, 
                triggerColliderData.GroundCheckCollider.transform.rotation, 
                LayerMasks.WalkableMask, 
                QueryTriggerInteraction.Ignore);
            
            return checkBox;
        }
        
        protected virtual void OnFall()
        {
            Fsm.ChangeState(Fsm.FallingState);
        }

        protected override void OnMovementPerformed(Vector2 value)
        {
            base.OnMovementPerformed(value);

            UpdateTargetRotation(GetMovementInputDirection());
        }
    }
}