using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;

namespace Core.Player.MovementFSM.States.Airborne
{
	public class HardFallingState : PlayerFallingState
	{
		public override bool CanExit => true;

		public HardFallingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
		{
			
		}
		
		public override void Enter()
		{
			base.Enter();
			
			if (Fsm.HasConstraint(AdapterConstraint.Falling))
			{
				Fsm.ChangeState(Fsm.FallingState);
				return;
			}
			
			//StartAnimation(AHash.IsHardFallingParameterHash);

			Fsm.ReusableData.MovementSpeedModifier = 0;
		}

		protected override void ExitFalling()
		{
			Fsm.ChangeState(Fsm.UnpinnedPuppetState);
			var dmg = new DamageArgs { Damage = 1000, DismemberDamage = 0, MetaDamageSource = new MetaDamageSource("Fall")};
			
			if (Fsm.Player.CurrentContext)
				Fsm.Player.CurrentContext.DoMassDamage(ref dmg);
		}

		protected override void EnterHardFalling()
		{
			
		}
		
	}
}