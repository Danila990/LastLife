using System;
using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.Player.MovementFSM.Data;
using Core.Player.MovementFSM.Mech.States;
using UniRx.Triggers;

namespace Core.Player.MovementFSM
{
    public class MechMovementStateMachine : StateMachine.StateMachine
    {
        public PlayerStateReusableData ReusableData { get; } = new ();
        public MechCharacterAdapter Player { get; }
        public PlayerMovementFsmData Data { get; }
        public MechWalkingState WalkingState { get; }
        public MechIdleState IdlingState { get; }
        public MechAirborneState AirborneState { get; }
        public MechLandingState LandingState { get; }
        public MechJumpingState JumpingState { get; }
        public MechFallingState FallingState { get; }
        public MechStationaryAttackState StationaryAttackState { get; }
        
        public MechMovementStateMachine(MechCharacterAdapter player, PlayerMovementFsmData data)
        {
            Player = player;
            Data = data;
            WalkingState = new MechWalkingState(this);
            IdlingState = new MechIdleState(this);
            AirborneState = new MechAirborneState(this);
            LandingState = new MechLandingState(this);
            JumpingState = new MechJumpingState(this);
            FallingState = new MechFallingState(this);
            StationaryAttackState = new MechStationaryAttackState(this);
        }
        
        public void ContextChanged(MechEntityContext characterContext)
        {
            if (!characterContext)
                return;
            characterContext.CharacterAnimator.ObserveAnimationEvents();
            //Player.CurrentContext.RigProvider.Rigs["aim"].DisableRig();
            ReusableData.IsAiming = false;
                
            characterContext.CharacterAnimator.AnimationTrigger += AnimationTrigger;
            characterContext.CharacterAnimator.AnimationTriggerExit += AnimStateExit;
            characterContext.CharacterAnimator.AnimationTriggerEnter += AnimStateEnter;
            Player.Rigidbody.isKinematic = false;
        }
        
        public bool HasConstraint(AdapterConstraint constraint)
        {
            if (!Player || !Player.CurrentContext)
                return false;

            return (Player.Constraints & constraint) != 0;
        }
        
        public void UseAnimationItem(Action action)
        {
            StationaryAttackState.Callback = action;
            CurrentState?.UseAction();
        }
        
        private void AnimStateEnter(ObservableStateMachineTrigger.OnStateInfo obj)
        {
            OnAnimationEnterEvent();
        }

        private void AnimStateExit(ObservableStateMachineTrigger.OnStateInfo obj)
        {
            OnAnimationExitEvent();
        }
        
        private void AnimationTrigger(string triggerKey)
        {
            OnAnimationTriggerEvent(triggerKey);
        }
    }
}