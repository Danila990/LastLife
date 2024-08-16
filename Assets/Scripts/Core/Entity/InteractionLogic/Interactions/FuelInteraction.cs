using System;
using Adv.Services.Interfaces;
using Core.Entity.Characters;
using Core.Fuel;
using GameSettings;
using SharedUtils;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public sealed class FuelInteraction : ItemSupplyInteraction
	{
		[SerializeField] private Sprite _uiIcon;
		[SerializeField] private OutlineHighLight _outline;
		[SerializeField] private Transform _equipmentObject;
		[SerializeField] private Vector3 _rotationVelocity;
		[SerializeField] private float _cooldownDuration;
		[SerializeField] private FuelTank _tank;
		
		[Inject] private readonly IAdvService _advService;
		[Inject] private readonly ITicketService _ticketService;
		[Inject] private readonly IFuelService _fuelService;
		[Inject] private readonly ISettingsService _settingsService;

		private WorldSpaceSupplyBox _currentButton;
		private AdvWorldButtonPresenter _advPresenter;
		private TicketWorldButtonPresenter _ticketPresenter;
		private IDisposable _cooldown;


		protected override void OnStart()
		{
			_advPresenter = new AdvWorldButtonPresenter(_advService, Callback, "Take supply:" + "Fuel tank");
			_ticketPresenter = new TicketWorldButtonPresenter(_ticketService, Callback, "Take supply:" + "Fuel tank");
			_outline.Init();
			_outline.Enable();
		}

		private void OnEnable()
		{
			_outline.Enable();
		}

		protected override void Select(CharacterContext characterContext)
		{
			_fuelService.AddTank(_tank);
			OnCooldown();
		}
		protected override void OnPlayerExit()
		{
			DisableUI();
			//_outline.Disable();
		}
    
		protected override void OnPlayerEnter()
		{
			DisableUI();
			ShowUI();
			//_outline.Enable();
		}
		
		protected override void OnUpdate()
		{
			_equipmentObject.Rotate(_rotationVelocity);
			if(!_currentButton) return;
			var cameraTransform = CameraService.CurrentBrain.OutputCamera.transform;
			var cameraDelta = cameraTransform.position - transform.position;
			CurrCamDist = cameraDelta.magnitude;
			_currentButton.transform.localScale = Vector3.one * 5 / Mathf.Max(1,CurrCamDist);
		}
		
		protected override void ShowUI()
		{
			_currentButton = WorldSpaceUIService.GetUI<WorldSpaceSupplyBox>(_worldButtonKey);
			_advPresenter.Attach(_currentButton.Button);
			_currentButton.Count.text = "";
			_currentButton.Count.gameObject.SetActive(false);

			_currentButton.Icon_holder.sprite = _uiIcon;
			_ticketPresenter.Attach(_currentButton.TicketButton);
			_currentButton.Target = transform;
			_currentButton.Offset = Offset;
		}
    
		protected override void DisableUI()
		{
			if (!_currentButton)
				return;
			
			_currentButton.Count.gameObject.SetActive(true);
			_currentButton.IsInactive = true;
			_advPresenter?.Dispose();
			_ticketPresenter?.Dispose();
			_currentButton = null;
		}
    
		protected override void OnDisposed()
		{
			_advPresenter?.Dispose();
			_ticketPresenter?.Dispose();
			_cooldown?.Dispose();
		}
    
		public override void Callback()
		{
			Use(PlayerSpawnService.PlayerCharacterAdapter.CurrentContext);
		}
		
		private void OnCooldown()
		{
			gameObject.SetActive(false);
			_cooldown?.Dispose();
			_cooldown = Observable.Timer(_cooldownDuration.ToSec())
				.Finally(() =>
				{
					_cooldown = null;
				})
				.Subscribe(_ =>
				{
					if(gameObject)
						gameObject.SetActive(true);
				});
		}
	}
}
