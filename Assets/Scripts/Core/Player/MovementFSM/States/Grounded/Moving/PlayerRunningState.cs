using Core.Boosts;
using Core.Entity.Characters;
using UnityEngine;
using Utils.Constants;

namespace Core.Player.MovementFSM.States.Grounded.Moving
{
	public class PlayerRunningState : PlayerMovingState
	{
		private float _timeEnter;
		
		public PlayerRunningState(PlayerMovementStateMachine fsm) : base(fsm)
		{
			
		}
		
#region IState
		public override void Enter()
		{
			var additionalValue = Fsm.ReusableData.StatsProvider.Stats.GetValue(StatType.MovementSpeed);
			Fsm.ReusableData.MovementSpeedModifier = GroundedData.RunData.SpeedModifier + additionalValue;

			base.Enter();
			StartAnimation(AHash.RunParameterHash);

			Fsm.ReusableData.CurrentJumpForce = AirborneData.JumpData.MediumForce;

			_timeEnter = Time.time;
		}
		
		public override void Exit()
		{
			base.Exit();

			StopAnimation(AHash.RunParameterHash);
		}

		public override void Update()
		{
			base.Update();

			if (_timeEnter + GroundedData.PlayerSprintData.RunToSprintTime < Time.time && Fsm.Player.InputDto.Move.y > .5f)
			{
				Fsm.ReusableData.ShouldSprint = true;
				Fsm.ChangeState(Fsm.SprintingState);
			}
		}
#endregion
		
#region Input Methods

		protected override void OnWalkToggleDown()
		{
			base.OnWalkToggleDown();
			
			Fsm.ChangeState(Fsm.WalkingState);
		}
		
		protected override void OnMovementCanceled()
		{
			base.OnMovementCanceled();
			
			Fsm.ChangeState(Fsm.IdlingState);
		}
#endregion
	}
}