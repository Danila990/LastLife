using Core.Boosts;
using Core.Entity.Characters;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Moving
{
	public class PlayerSprintingState : PlayerMovingState
	{

		public PlayerSprintingState(PlayerMovementStateMachine fsm) : base(fsm)
		{
			
		}

		public override void Enter()
		{
			base.Enter();
			
			var additionalValue = Fsm.ReusableData.StatsProvider.Stats.GetValue(StatType.MovementSpeed);
			Fsm.ReusableData.MovementSpeedModifier = GroundedData.PlayerSprintData.SpeedModifier + additionalValue;
			Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.StrongForce;
 			
			StartAnimation(AHash.SprintParameterHash);
		}

		public override void Exit()
		{
			base.Exit();

			StopAnimation(AHash.SprintParameterHash);
		}

		public override void Update()
		{
			base.Update();
			
			if (Fsm.ReusableData.ShouldWalk || Fsm.ReusableData.IsAiming)
			{
				Fsm.ChangeState(Fsm.WalkingState);
				return;
			}
		}

		protected override void OnMovementCanceled()
		{
			base.OnMovementCanceled();
			
			Fsm.ChangeState(Fsm.RunningState);
		}
	}
}