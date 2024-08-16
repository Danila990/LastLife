using Core.Entity.Characters.Adapters;
using Utils.Constants;

namespace Core.Player.MovementFSM.Mech.States
{
    public class MechWalkingState : MechMovingState
    {
        public MechWalkingState(MechMovementStateMachine fsm) : base(fsm)
        {
        			
        }
        
        #region IState
        public override void Enter()
        {
            Fsm.ReusableData.MovementSpeedModifier = GroundedData.WalkData.SpeedModifier;
        
            base.Enter();
        
            StartAnimation(AHash.WalkParameterHash);
        
            Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.WeakForce;
        }
        
        public override void Exit()
        {
            base.Exit();
        
            StopAnimation(AHash.WalkParameterHash);
        
        }
        #endregion
        
        
        public override void Update()
        {
            base.Update();
        }
        		
        #region Input Methods
        protected override void OnWalkToggleDown()
        {
            base.OnWalkToggleDown();
        			
            //Fsm.ChangeState(Fsm.WalkingState);
        }
        		
        protected override void OnMovementCanceled()
        {
            base.OnMovementCanceled();
        			
            Fsm.ChangeState(Fsm.IdlingState);
        }
        #endregion
        
        public void SetConstraints()
        {
            Fsm.Player.Constraints =
                AdapterConstraint.Falling
                | AdapterConstraint.Jumping
                | AdapterConstraint.Movement
                | AdapterConstraint.Rotation;
        }
        public void ResetConstraints()
        {
            Fsm.Player.Constraints = AdapterConstraint.None;
        }
    }
}