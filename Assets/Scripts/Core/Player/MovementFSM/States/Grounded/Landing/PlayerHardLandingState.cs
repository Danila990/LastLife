using Core.AnimationTriggers;
using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Landing
{
    public class PlayerHardLandingState : PlayerLandingState
    {
        public PlayerHardLandingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            Fsm.ReusableData.MovementSpeedModifier = 0f;

            var velocity = Fsm.ReusableData.VerticalVelocity;
            base.Enter();
            if (Fsm.HasConstraint(AdapterConstraint.Falling))
            {
                Fsm.ChangeState(Fsm.WalkingState);
                return;
            }


            var damageThreshold = Fsm.Data.PlayerAirborneData.FallData.DamageVelocityThreshold;
            var deathThreshold = Fsm.Data.PlayerAirborneData.FallData.HardFallingVelocityThreshold;
            var maxHealth = Fsm.Player.CurrentContext.Health.MaxHealth;

            if (velocity.y < -damageThreshold)
            {
                var p = Mathf.Abs(velocity.y) - damageThreshold;
                var interval = deathThreshold - damageThreshold;
                var percent = p / interval;
                var args = new DamageArgs { Damage = maxHealth * percent, DismemberDamage = 1000 ,MetaDamageSource = new MetaDamageSource("Fall")};
                Fsm.Player.CurrentContext.DoMassDamage(ref args);
            }
            StartAnimation(AHash.HardLandParameterHash);
            ResetVelocity();
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(AHash.HardLandParameterHash);
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

        public override void OnAnimationExitEvent()
        {
            //Fsm.Player.Input.PlayerActions.Movement.Enable();
            Fsm.ChangeState(Fsm.IdlingState);
        }

        public override void OnAnimationTriggerEvent(string key)
        {
            if (key == nameof(LandingExitTriggerBehaviour))
            {
                Fsm.ChangeState(Fsm.IdlingState);
            }
        }

        protected override void OnMove()
        {
            if (Fsm.ReusableData.ShouldWalk)
            {
                return;
            }

            Fsm.ChangeState(Fsm.RunningState);
        }

        protected override void OnJumpStarted()
        {
            
        }
    }
}