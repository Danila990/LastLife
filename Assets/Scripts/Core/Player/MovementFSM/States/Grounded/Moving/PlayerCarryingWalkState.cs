using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Moving
{
	public class PlayerCarryingWalkState : PlayerMovingState
	{
		private bool _forceExit;
		public override bool CanExit 
			=> Fsm.ReusableData.IsDead || _forceExit || !Fsm.ReusableData.CarryInventory || !Fsm.ReusableData.CarryInventory.HasContext;
		
		public PlayerCarryingWalkState(PlayerMovementStateMachine fsm) : base(fsm)
		{
			
		}

#region IState
		public override void Enter()
		{
			Fsm.ReusableData.MovementSpeedModifier = GroundedData.CarryingData.SpeedModifier;
			
			base.Enter();

			StartAnimation(AHash.WalkParameterHash);

			Fsm.ReusableData.CurrentJumpForce = Vector3.zero;
		}

		public override void Exit()
		{
			base.Exit();

			StopAnimation(AHash.WalkParameterHash);
		}
#endregion

		
#region Input Methods
		protected override void OnMovementCanceled()
		{
			base.OnMovementCanceled();

			_forceExit = true;
			Fsm.ChangeState(Fsm.ReusableData.CarryInventory.HasContext ? Fsm.CarryingIdleState : Fsm.IdlingState);
			_forceExit = false;
		}
#endregion
		
	}
}
