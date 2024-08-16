using Core.Entity.Characters.Adapters;
using Core.Player.MovementFSM.Data;
using Core.Player.MovementFSM.Data.Airborne;
using StateMachine;
using UnityEngine;
using Utils;

namespace Core.Player.MovementFSM.States
{
	public class PlayerMovementState : IState
	{
		public virtual bool CanExit => true;
		
		protected bool CanMove => !Fsm.Player.Constraints.HasFlag(AdapterConstraint.Movement);
		
		protected PlayerMovementStateMachine Fsm { get; }
		protected PlayerGroundedData GroundedData { get; set; }
		protected PlayerAirborneData AirborneData { get; set; }

		public PlayerMovementState(PlayerMovementStateMachine fsm)
		{
			Fsm = fsm;

			GroundedData = fsm.Data.PlayerGroundedData;
			AirborneData = fsm.Data.PlayerAirborneData;

			InitializeData();
		}

		private void InitializeData()
		{
			SetBaseRotationData();
		}
		
		protected void SetBaseRotationData()
		{
			Fsm.ReusableData.RotationData = Fsm.Data.PlayerGroundedData.RotationData;
			Fsm.ReusableData.TimeToReachTargetRotation = GroundedData.RotationData.TargetRotationReachTime;
		}

#region PublicMethods
		public virtual void Enter()
		{
			AddInputActionsCallbacks();
		}
		
		public virtual void Exit()
		{
			RemoveInputActionsCallbacks();
		}
		
		public virtual void HandleInput()
		{
			ReadMovementInput();

			if (Fsm.Player.InputDto.MoveRaw == Vector2.zero)
			{
				OnMovementCanceled();
			}
			else 
			{
				OnMovementPerformed(Fsm.Player.InputDto.Move);
			}
		}
		public virtual void Update() { }
		public virtual void LateUpdate() { }
		public virtual void PhysicsUpdate()
		{
			Move();
		}
		
		public virtual void OnTriggerEnter(Collider collider)
		{
			if (Layers.ContainsLayer(LayerMasks.WalkableMask, collider.gameObject.layer))
			{
				OnContactWithGround(collider);
			}
		}
		public virtual void OnTriggerExit(Collider collider)
		{
			if (Layers.ContainsLayer(LayerMasks.WalkableMask, collider.gameObject.layer))
			{
				OnContactWithGroundExited(collider);
			}
		}
		public virtual  void OnAnimationEnterEvent() { }
		public virtual void OnAnimationExitEvent() { }
		public virtual void OnAnimationTriggerEvent(string key) { }
		
		public virtual void OnLoseBalance() => Fsm.ChangeState(Fsm.UnpinnedPuppetState);
		public virtual void OnRegainBalance() => Fsm.ChangeState(Fsm.IdlingState);
		
		public virtual void UseAction()
		{
			Fsm.ChangeState(Fsm.StationaryAttackState);
		}
#endregion

#region MainMethods

		private void ReadMovementInput()
		{
			Fsm.ReusableData.MovementInput = CanMove ? Fsm.Player.InputDto.Move : Vector2.zero;
		}

		private void Move()
		{
			if (Fsm.ReusableData.MovementInput == Vector2.zero || Fsm.ReusableData.MovementSpeedModifier == 0f)
			{
				if (Fsm.ReusableData.IsAiming)
				{
					Rotate(Vector3.zero);
				}
				return;
			}
			
			var movementDirection = GetMovementInputDirection();
			var targetRotationYAngle = Rotate(movementDirection);
			var targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);
			var movementSpeed = GetMovementSpeed();
			var currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
			Fsm.Player.Rigidbody.AddForce(targetRotationDirection * movementSpeed - currentPlayerHorizontalVelocity, ForceMode.VelocityChange);
		}
		
		protected Vector3 GetMovementInputDirection()
		{
			return new Vector3(Fsm.ReusableData.MovementInput.x, 0f, Fsm.ReusableData.MovementInput.y);
		}
		
		private float Rotate(Vector3 direction)
        {
            var directionAngle = UpdateTargetRotation(direction);

            RotateTowardsTargetRotation();

            return directionAngle;
        }
#endregion

#region ReuseableMethods
		
		protected void StartAnimation(int animationHash)
		{
			if (!Fsm.Player.CurrentContext)
				return;
			
			Fsm.Player.CurrentContext.CharacterAnimator.Animator.SetBool(animationHash, true);

			if (Fsm.Player.CurrentContext.Inventory.SelectedItem && Fsm.Player.CurrentContext.Inventory.SelectedItem.ItemAnimator)
				Fsm.Player.CurrentContext.Inventory.SelectedItem.ItemAnimator.PlayAnim(animationHash);
		}

		protected void StopAnimation(int animationHash)
		{
			if (Fsm.Player.CurrentContext)
			{
				Fsm.Player.CurrentContext.CharacterAnimator.Animator.SetBool(animationHash, false);
			}
		}
		
		protected virtual void AddInputActionsCallbacks()
		{
			Fsm.ReusableData.OnSprintDown += OnWalkToggleDown;

			//Fsm.Player.InputDto.Input.PlayerActions.Look.started += OnMouseMovementStarted;
			//Fsm.Player.InputDto.Input.PlayerActions.Movement.performed += OnMovementPerformed;
			//Fsm.Player.InputDto.Input.PlayerActions.Movement.canceled += OnMovementCanceled;
		}

		protected virtual void RemoveInputActionsCallbacks()
		{
			Fsm.ReusableData.OnSprintDown -= OnWalkToggleDown;

			//Fsm.Player.Input.PlayerActions.Look.started -= OnMouseMovementStarted;
			//Fsm.Player.Input.PlayerActions.Movement.performed -= OnMovementPerformed;
			//Fsm.Player.Input.PlayerActions.Movement.canceled -= OnMovementCanceled;
		}
		
		protected virtual void OnWalkToggleDown()
		{
			Fsm.ReusableData.ShouldWalk = !Fsm.ReusableData.ShouldWalk;
		}

		private void OnMouseMovementStarted(Vector2 value)
		{
			//UpdateCameraRecenteringState(stateMachine.ReusableData.MovementInput);
		}

		protected virtual void OnMovementPerformed(Vector2 value)
		{
			//UpdateCameraRecenteringState(context.ReadValue<Vector2>());
		}

		protected virtual void OnMovementCanceled()
		{
			//DisableCameraRecentering();
		}

        protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
        {
            var directionAngle = GetDirectionAngle(direction);

            if (shouldConsiderCameraRotation)
            {
                directionAngle = AddCameraRotationToAngle(directionAngle);
            }
            
            if (Fsm.ReusableData.IsAiming)
            {
	            direction = Vector3.ProjectOnPlane(Fsm.Player.MainCameraTransform.forward, Vector3.up).normalized;
	            UpdateTargetRotationData(GetDirectionAngle(direction));
            }		
            else if (!Mathf.Approximately(directionAngle, Fsm.ReusableData.CurrentTargetRotation.y))
            {
			    UpdateTargetRotationData(directionAngle);
            }

            return directionAngle;
        }

        protected void RotateTowardsTargetRotation()
        {
            var currentYAngle = Fsm.Player.Rigidbody.rotation.eulerAngles.y;

            if (Mathf.Approximately(currentYAngle, Fsm.ReusableData.CurrentTargetRotation.y))
            {
                return;
            }

            var smoothedYAngle = Mathf.SmoothDampAngle(
	            currentYAngle,
	            Fsm.ReusableData.CurrentTargetRotation.y, 
	            ref Fsm.ReusableData.DampedTargetRotationCurrentVelocity.y, 
	            Fsm.ReusableData.TimeToReachTargetRotation.y - Fsm.ReusableData.DampedTargetRotationPassedTime.y);

            Fsm.ReusableData.DampedTargetRotationPassedTime.y += Time.fixedDeltaTime;

            var targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            Fsm.Player.Rigidbody.MoveRotation(targetRotation);
        }

        protected Vector3 GetTargetRotationDirection(float targetRotationAngle)
        {
            return Quaternion.Euler(0f, targetRotationAngle, 0f) * Vector3.forward;
        }

        protected float GetMovementSpeed(bool shouldConsiderSlopes = true)
        {
            var movementSpeed = GroundedData.BaseSpeed * Fsm.ReusableData.MovementSpeedModifier;

            if (shouldConsiderSlopes)
            {
                movementSpeed *= Fsm.ReusableData.MovementOnSlopesSpeedModifier;
            }

            // if (Fsm.ReusableData.IsAiming)
            // {
	           //  movementSpeed *= GroundedData.AimMoveSpeedMlp;
            // }

            return movementSpeed;
        }

        protected Vector3 GetPlayerHorizontalVelocity()
        {
            var playerHorizontalVelocity = Fsm.Player.Rigidbody.velocity;

            playerHorizontalVelocity.y = 0f;

            return playerHorizontalVelocity;
        }

        protected Vector3 GetPlayerVerticalVelocity()
        {
            return new Vector3(0f, Fsm.Player.Rigidbody.velocity.y, 0f);
        }
        
        protected void ResetVelocity()
        {
	        Fsm.Player.Rigidbody.velocity = Vector3.zero;
	        Fsm.ReusableData.VerticalVelocity = Fsm.Player.Rigidbody.velocity;
        }

        protected void ResetVerticalVelocity()
        {
	        Fsm.Player.Rigidbody.velocity = GetPlayerHorizontalVelocity();
	        Fsm.ReusableData.VerticalVelocity = Fsm.Player.Rigidbody.velocity;
        }

#endregion
		
#region SubMethods

		private float GetDirectionAngle(Vector3 direction)
		{
			var directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

			if (directionAngle < 0f)
			{
				directionAngle += 360f;
			}

			return directionAngle;
		}

		private float AddCameraRotationToAngle(float angle)
		{
			angle += Fsm.Player.MainCameraTransform.eulerAngles.y;

			if (angle > 360f)
			{
				angle -= 360f;
			}

			return angle;
		}

		private void UpdateTargetRotationData(float targetAngle)
		{
			Fsm.ReusableData.CurrentTargetRotation.y = targetAngle;

			Fsm.ReusableData.DampedTargetRotationPassedTime.y = 0f;
		}
		
		protected virtual void OnContactWithGroundExited(Collider collider)
		{
			
		}
		
		protected void DecelerateHorizontally()
		{
			var playerHorizontalVelocity = GetPlayerHorizontalVelocity();

			Fsm.Player.Rigidbody.AddForce(-playerHorizontalVelocity * Fsm.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
		}

		protected void DecelerateVertically()
		{
			var playerVerticalVelocity = GetPlayerVerticalVelocity();

			Fsm.Player.Rigidbody.AddForce(-playerVerticalVelocity * Fsm.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
		}

		protected bool IsMovingHorizontally(float minimumMagnitude = 0.1f)
		{
			var playerHorizontaVelocity = GetPlayerHorizontalVelocity();

			var playerHorizontalMovement = new Vector2(playerHorizontaVelocity.x, playerHorizontaVelocity.z);

			return playerHorizontalMovement.magnitude > minimumMagnitude;
		}

		protected bool IsMovingUp(float minimumVelocity = 0.1f)
		{
			return GetPlayerVerticalVelocity().y > minimumVelocity;
		}

		protected bool IsMovingDown(float minimumVelocity = 0.1f)
		{
			return GetPlayerVerticalVelocity().y < -minimumVelocity;
		}
#endregion
		protected virtual void OnContactWithGround(Collider collider)
		{
		}
	}
}