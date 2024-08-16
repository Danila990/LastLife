using System;
using Core.Boosts;
using Core.Entity.Characters;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Player.MovementFSM.States.Airborne
{
    public class PlayerJumpingState : PlayerAirborneState
    {
        private IDisposable _timer;
        
        private bool _canStartFalling;

        public PlayerJumpingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

#region IState
        public override void Enter()
        {
            base.Enter();
            Fsm.ReusableData.InJump = true;
            AddInputListeners();
            Fsm.ReusableData.RotationData = AirborneData.JumpData.RotationData;
            if (Fsm.ReusableData.MovementSpeedModifier == 0)
            {
                Fsm.ReusableData.MovementSpeedModifier = 0.25f;
            }

            Jump();
            _timer?.Dispose();
            _timer = Observable
                .Timer(0.5f.ToSec())
                .Subscribe(_ =>
                {
                    Fsm.ChangeState(Fsm.JetPackState);
                });
        }

        public override void Exit()
        {
            Fsm.ReusableData.InJump = false;
            DisposeTimer();
            base.Exit();
            SetBaseRotationData();
            RemoveInputListeners();

            _canStartFalling = false;
        }

        public override void Update()
        {
            base.Update();

            if (!_canStartFalling && IsMovingUp(0f))
            {
                _canStartFalling = true;
            }

            if (!_canStartFalling || IsMovingUp(0f))
            {
                return;
            }

            Fsm.ChangeState(Fsm.FallingState);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            RotateTowardsTargetRotation();

            if (IsMovingUp())
            {
                DecelerateVertically();
            }
        }
#endregion

        private void AddInputListeners()
        {
            Fsm.ReusableData.OnJumpUp += DisposeTimer;
        }

        private void RemoveInputListeners()
        {
            Fsm.ReusableData.OnJumpUp -= DisposeTimer;
        }

        private void DisposeTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }
        
        private void Jump()
        {
            var jumpForce = Fsm.ReusableData.CurrentJumpForce;
            jumpForce.y += Fsm.ReusableData.StatsProvider.Stats.GetValue(StatType.JumpForce);
            UpdateTargetRotation(GetMovementInputDirection());

            var jumpDirection = GetTargetRotationDirection(Fsm.ReusableData.CurrentTargetRotation.y) * AirborneData.JumpData.RotateSpeedOnJump;

            jumpForce.x *= jumpDirection.x;
            jumpForce.z *= jumpDirection.z;

            jumpForce = GetJumpForceOnSlope(jumpForce);

            ResetVelocity();

            Fsm.Player.Rigidbody.AddForce(jumpForce, ForceMode.VelocityChange);
        }

        private Vector3 GetJumpForceOnSlope(Vector3 jumpForce)
        {
            var capsuleColliderCenterInWorldSpace = Fsm.Player.CapsuleColliderUtility.CapsuleColliderData.Collider.bounds.center;

            var downwardsRayFromCapsuleCenter = new Ray(capsuleColliderCenterInWorldSpace, Vector3.down);

            if (Physics.Raycast(downwardsRayFromCapsuleCenter, out var hit, AirborneData.JumpData.JumpToGroundRayDistance, LayerMasks.WalkableMask, QueryTriggerInteraction.Ignore))
            {
                var groundAngle = Vector3.Angle(hit.normal, -downwardsRayFromCapsuleCenter.direction);

                if (IsMovingDown())
                {
                    var forceModifier = AirborneData.JumpData.JumpForceModifierOnSlopeDownwards.Evaluate(groundAngle);

                    jumpForce.y *= forceModifier;
                }
            }

            return jumpForce;
        }

        protected override void ResetSprintState()
        {
        }

        protected void OnMovementCanceled(Vector2 value)
        {
        }
    }
}