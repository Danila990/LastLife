using System;
using Core.Carry;
using Core.Entity.Characters;
using Core.Equipment.Inventory;
using UniRx;
using UnityEngine;

namespace Core.Player.MovementFSM.Data
{
	public class PlayerStateReusableData
	{
		public Vector2 MovementInput { get; set; }
		public Vector3 TeleportPosition { get; set; }
		
		public bool IsThirdPersonView { get; set; }
		private bool _isAiming;
		public bool IsAiming
		{
			get => _isAiming && IsThirdPersonView;
			set => _isAiming = value;
		}

		public bool IsDead;
		public bool InJump;

		public float MovementSpeedModifier { get; set; } = 1f;
		public float MovementOnSlopesSpeedModifier { get; set; } = 1f;
		public float MovementDecelerationForce { get; set; } = 1f;

		public StatsProvider StatsProvider;

		public EquipmentInventory EquipmentInventory;
		public CarryInventory CarryInventory;
		
		public bool ShouldWalk { get; set; }
		public bool ShouldSprint { get; set; }

		private Vector3 _currentTargetRotation;
		private Vector3 _timeToReachTargetRotation;
		private Vector3 _dampedTargetRotationCurrentVelocity;
		private Vector3 _dampedTargetRotationPassedTime;

		public ref Vector3 CurrentTargetRotation => ref _currentTargetRotation;

		public ref Vector3 TimeToReachTargetRotation => ref _timeToReachTargetRotation;

		public ref Vector3 DampedTargetRotationCurrentVelocity => ref _dampedTargetRotationCurrentVelocity;

		public ref Vector3 DampedTargetRotationPassedTime => ref _dampedTargetRotationPassedTime;

		public Vector3 CurrentJumpForce { get; set; }
		public PlayerRotationData RotationData { get; set; }
		public Action OnSprintDown;
		public Action OnJumpDown;
		public Action OnJumpUp;
		
		public BoolReactiveProperty InAir { get; set; } = new ();
		public Vector3 VerticalVelocity { get; set; }
	}
}