using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic.Animation;
using Sirenix.OdinInspector;
using Ui.Sandbox.PlayerInput;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class RocketLauncherInteraction : StaticWeaponInteraction, ICameraTargetEntity
	{
		[Inject] private readonly IRayCastService _rayCastService;
		[Inject] private readonly PlayerInputController _playerInputController;
		[TitleGroup("RocketLauncherInteraction")]
		[SerializeField] private string _worldButtonWithoutAdvKey;
		[SerializeField] private LineRenderer _laserLine;
		[SerializeField] private StaticCharacterAnimator _staticCharacterAnimator;
		[SerializeField] private Collider _rocketCollider;
		[field:SerializeField] public Transform CameraTargetRoot { get; set; }
		[field:SerializeField] public Transform ShootOrigin { get; set; }
		
		public bool TargetIsActive => true;
		public float AdditionalCameraDistance { get; set; } = 0f;
		private WorldButtonPresenter _defaultWorldButton;

		protected override void OnStart()
		{
			base.OnStart();
			_defaultWorldButton = new WorldButtonPresenter(Callback);
		}

		protected override void OnUpdateOnActive()
		{
			RigTarget.position = 
				Vector3.Lerp(RigTarget.position, 
					_rayCastService.CurrentHitPoint, Time.deltaTime * 10);
			
			UpdateLaserPositions();
		}
	
		protected override void OnAttached()
		{
			UpdateLaserPositions();
			_laserLine.enabled = true;
			CameraService.SetTrackedTarget(this);
			_playerInputController.OnAim(new AimStatus(AimState.Aim));
			_staticCharacterAnimator.AttachCharacter(AttachedAdapter.CurrentContext, AttachedAdapter.transform);
			_rocketCollider.enabled = false;
		}

		protected override void OnDetached()
		{
			_rocketCollider.enabled = true;
			_laserLine.enabled = false;
			_staticCharacterAnimator.Detach();
		}

		protected override void ShowUI()
		{
			if (StaticItemContext.CurrentQuantity.Value > 0)
			{
				CurrentUI = WorldSpaceUIService.GetUI<WorldSpaceButton>(_worldButtonWithoutAdvKey);
				_defaultWorldButton.Attach(CurrentUI.Button);
				CurrentUI.Target = _uiOrigin;
				CurrentUI.Offset = Vector3.zero;
			}
			else
			{
				base.ShowUI();
			}
		}

		protected override void OnDisableUI()
		{
			_defaultWorldButton.Dispose();
		}
		
		private void UpdateLaserPositions()
		{
			var triggered = Physics.Raycast(ShootOrigin.position, ShootOrigin.forward, out var hit, 50);
			Vector3 endPoint; 
			if (!triggered)
			{
				endPoint = ShootOrigin.position + ShootOrigin.forward * 50;
			}
			else
			{
				endPoint = hit.point;
			}
			_laserLine.SetPosition(1, ShootOrigin.InverseTransformPoint(endPoint));
		}
	}
}
