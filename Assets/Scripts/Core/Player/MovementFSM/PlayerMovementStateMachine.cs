using System;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Player.MovementFSM.Data;
using Core.Player.MovementFSM.States;
using Core.Player.MovementFSM.States.Airborne;
using Core.Player.MovementFSM.States.Grounded;
using Core.Player.MovementFSM.States.Grounded.Attack;
using Core.Player.MovementFSM.States.Grounded.Landing;
using Core.Player.MovementFSM.States.Grounded.Moving;
using Core.Player.MovementFSM.States.Grounded.Stopping;
using Core.Player.MovementFSM.States.Interaction;
using UniRx.Triggers;
using UnityEngine;

namespace Core.Player.MovementFSM
{
    public class PlayerMovementStateMachine : StateMachine.StateMachine
    {
        private static readonly int IsAiming = Animator.StringToHash("isAiming");
        public PlayerStateReusableData ReusableData { get; } = new PlayerStateReusableData();
        public PlayerCharacterAdapter Player { get; }
        public PlayerMovementFsmData Data { get; }
        public PlayerIdlingState IdlingState { get; }
        public PlayerWalkingState WalkingState { get; }
        public PlayerManualWalk ManualWalk { get; }
        public PlayerRunningState RunningState { get; }
        public PlayerSprintingState SprintingState { get; }
        
        public PlayerCarryingIdleState CarryingIdleState { get; }
        public PlayerCarryingWalkState CarryingWalkState { get; }

        public PlayerJumpingState JumpingState { get; }
        public PlayerFallingState FallingState { get; }
        public HardFallingState HardFallingState { get; }
        public PlayerAirborneState AirborneState { get; }
        public PlayerJetPackState JetPackState { get; }
        
        public PlayerZiplineState ZiplineState { get; }
        public PlayerTeleportState TeleportState { get; }
        
        public PlayerLightStoppingState LightStoppingState { get; }
        public PlayerMediumStoppingState MediumStoppingState { get; }
        public PlayerHardStoppingState HardStoppingState { get; }
        
        public PlayerLightLandingState LightLandingState { get; }
        public PlayerHardLandingState HardLandingState { get; }
        public UnpinnedPuppetState UnpinnedPuppetState { get; }
        public StationaryAttackState StationaryAttackState { get; }
        
        
        public PlayerMovementStateMachine(
            PlayerCharacterAdapter player,
            PlayerMovementFsmData data
            )
        {
            Player = player;
            Data = data;
            
            IdlingState = new PlayerIdlingState(this);
            
            WalkingState = new PlayerWalkingState(this);
            ManualWalk = new PlayerManualWalk(this);
            RunningState = new PlayerRunningState(this);
            SprintingState = new PlayerSprintingState(this);
            CarryingWalkState = new PlayerCarryingWalkState(this);
            CarryingIdleState = new PlayerCarryingIdleState(this);
            
            JumpingState = new PlayerJumpingState(this);
            FallingState = new PlayerFallingState(this);
            HardFallingState = new HardFallingState(this);
            AirborneState = new PlayerAirborneState(this);
            
            LightStoppingState = new PlayerLightStoppingState(this);
            MediumStoppingState = new PlayerMediumStoppingState(this);
            HardStoppingState = new PlayerHardStoppingState(this);
            
            LightLandingState = new PlayerLightLandingState(this);
            HardLandingState = new PlayerHardLandingState(this);
            
            UnpinnedPuppetState = new UnpinnedPuppetState(this);
            StationaryAttackState = new StationaryAttackState(this);
            ZiplineState = new PlayerZiplineState(this);
            JetPackState = new PlayerJetPackState(this);
            TeleportState = new PlayerTeleportState(this);
        }

        public void ContextChanged(CharacterContext characterContext)
        {
            if (!characterContext)
                return;
            characterContext.CharacterAnimator.ObserveAnimationEvents();
            characterContext.BehaviourPuppet.onLoseBalance.unityEvent.AddListener(OnLoseBalance);
            characterContext.BehaviourPuppet.onGetUpProne.unityEvent.AddListener(OnRegainBalance);
            characterContext.BehaviourPuppet.onGetUpSupine.unityEvent.AddListener(OnRegainBalance);
            Player.CurrentContext.RigProvider.Rigs["aim"].DisableRig();
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
        
        public void DisposeCharacterContext(CharacterContext characterContext)
        {
            Player.Rigidbody.isKinematic = true;
            characterContext.BehaviourPuppet.onLoseBalance.unityEvent.RemoveListener(OnLoseBalance);
            characterContext.BehaviourPuppet.onGetUpProne.unityEvent.RemoveListener(OnRegainBalance);
            characterContext.BehaviourPuppet.onGetUpSupine.unityEvent.RemoveListener(OnRegainBalance);
            ReusableData.IsAiming = false;
                
            characterContext.CharacterAnimator.AnimationTrigger -= AnimationTrigger;
            characterContext.CharacterAnimator.AnimationTriggerExit -= AnimStateExit;
            characterContext.CharacterAnimator.AnimationTriggerEnter -= AnimStateEnter;
        }
        
        public void OnAimingChange(bool status)
        {
            if(!Player.CurrentContext) 
                return;
            ReusableData.IsAiming = status;
            Player.CurrentContext.CharacterAnimator.Animator.SetBool(IsAiming, status);
        }

        private void OnRegainBalance()
        {
            CurrentState?.OnRegainBalance();
        }

        private void OnLoseBalance()
        {
            CurrentState?.OnLoseBalance();   
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
        
        public void UseAnimationItem(Action action)
        {
            StationaryAttackState.Callback = action;
            CurrentState?.UseAction();
        }
    }
}