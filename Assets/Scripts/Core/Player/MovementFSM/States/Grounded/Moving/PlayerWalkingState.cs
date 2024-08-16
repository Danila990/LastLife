
using Core.Entity.Characters.Adapters;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Moving
{
	public class PlayerWalkingState : PlayerMovingState
	{

		public PlayerWalkingState(PlayerMovementStateMachine fsm) : base(fsm)
		{
			
		}

#region IState
		public override void Enter()
		{
			Fsm.ReusableData.MovementSpeedModifier = GroundedData.WalkData.SpeedModifier;

			//Fsm.ReusableData.BackwardsCameraRecenteringData = GroundedData.WalkData.BackwardsCameraRecenteringData;

			base.Enter();

			StartAnimation(AHash.WalkParameterHash);

			Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.WeakForce;
		}

		public override void Exit()
		{
			base.Exit();

			StopAnimation(AHash.WalkParameterHash);

			//SetBaseCameraRecenteringData();
		}
#endregion


		public override void Update()
		{
			base.Update();
			if (!Fsm.ReusableData.IsAiming)
			{
				Fsm.ChangeState(Fsm.IdlingState);
			}
		}
		
#region Input Methods
		protected override void OnWalkToggleDown()
		{
			base.OnWalkToggleDown();
			
			Fsm.ChangeState(Fsm.RunningState);
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