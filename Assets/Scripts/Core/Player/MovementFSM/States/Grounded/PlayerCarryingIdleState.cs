using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded
{

	public class PlayerCarryingIdleState : PlayerGroundedState
	{

		private bool _forceExit;
		public override bool CanExit 
			=> Fsm.ReusableData.IsDead || _forceExit || !Fsm.ReusableData.CarryInventory || !Fsm.ReusableData.CarryInventory.HasContext;

		public PlayerCarryingIdleState(PlayerMovementStateMachine fsm) : base(fsm)
		{
		}
		
#region IState
		public override void Enter()
		{
			Fsm.ReusableData.MovementSpeedModifier = 0f;

			base.Enter();

			StartAnimation(AHash.IdleParameterHash);
			Fsm.ReusableData.CurrentJumpForce = Vector3.zero;
			ResetVelocity();
		}
		
		public override void Exit()
		{
			base.Exit();

			StopAnimation(AHash.IdleParameterHash);
		}

		public override void Update()
		{
			base.Update();

			if (Fsm.ReusableData.MovementInput == Vector2.zero)
			{
				return;
			}

			_forceExit = true;
			Fsm.ChangeState(Fsm.ReusableData.CarryInventory.HasContext ? Fsm.CarryingWalkState : Fsm.WalkingState);
			_forceExit = false;
		}
		
		public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();

			if (!IsMovingHorizontally(0))
			{
				return;
			}

			ResetVelocity();
		}
#endregion
	}
}