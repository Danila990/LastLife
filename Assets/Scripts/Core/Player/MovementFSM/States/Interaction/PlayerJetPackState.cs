using Core.Entity.Characters;
using Core.Equipment;
using Core.Equipment.Data;
using Core.Equipment.Impl.JetPack;
using Core.Equipment.Inventory;
using UnityEngine;

namespace Core.Player.MovementFSM.States.Interaction
{
	public class PlayerJetPackState : PlayerMovementState
	{
		
		private bool _inAir;

		public override bool CanExit => !_inAir || Fsm.ReusableData.IsDead;
		private EquipmentInventory Inventory => Fsm.ReusableData.EquipmentInventory;

		private JetPackContext _currentJetPack;
		
		public PlayerJetPackState(PlayerMovementStateMachine fsm) : base(fsm)
		{
		}

		public override void Enter()
		{
			if (!Inventory || Fsm.ReusableData.IsDead)
			{
				Fall();
				return;
			}

			if (Inventory.Controller.ActiveEquipment.TryGetActivePart(EquipmentPartType.JetPack, out _currentJetPack))
			{
				if (_currentJetPack.Args.CurrentFuel.Value <= 0)
				{
					Fall();
					return;
				}
			}
			else
			{
				Fall();
				return;
			}


			Fsm.ReusableData.MovementSpeedModifier = _currentJetPack.Args.MoveSpeedModifier + Fsm.ReusableData.StatsProvider.Stats.GetValue(StatType.MovementSpeed);
			_inAir = true;
			_currentJetPack.OnLaunch();
			base.Enter();
		}

		public override void Exit()
		{
			if (_currentJetPack)
				_currentJetPack.OnStop();
			
			_currentJetPack = null;
			base.Exit();
		}

		protected override void AddInputActionsCallbacks()
		{
			base.AddInputActionsCallbacks();
			Fsm.ReusableData.OnJumpUp += StopFlying;
		}

		protected override void RemoveInputActionsCallbacks()
		{
			base.RemoveInputActionsCallbacks();
			Fsm.ReusableData.OnJumpUp -= StopFlying;
		}

		public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();
			
			UpdateTargetRotation(GetMovementInputDirection());

			var preferredDirection = GetTargetRotationDirection(Fsm.ReusableData.CurrentTargetRotation.y) * AirborneData.JumpData.RotateSpeedOnJump;
			
			if (!_currentJetPack.Fly(Time.fixedDeltaTime, preferredDirection))
			{
				StopFlying();
			}
		}

		private void Fall()
		{
			_inAir = false;
			Fsm.ChangeState(Fsm.FallingState);
		}
		
		private void StopFlying()
		{
			_inAir = false;
			Fsm.ChangeState(Fsm.FallingState);
		}

	}
}
