using System;
using Adv.Services.Interfaces;
using Core.Entity.Characters;
using Core.Equipment;
using Core.Equipment.Data;
using Core.Services;
using GameSettings;
using SharedUtils;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public sealed class EquipmentItemInteraction : ItemSupplyInteraction
	{
		[SerializeField] private Sprite _uiIcon;
		[SerializeField] private OutlineHighLight _outline;
		[SerializeField] private Transform _equipmentObject;
		[SerializeField] private Vector3 _rotationVelocity;
		[SerializeField] private float _cooldownDuration;
		[SerializeField] private string _equipmentDataId;
		
		[Inject] private readonly IEquipmentInventoryService _equipmentInventoryService;
		[Inject] private readonly IItemStorage _storage;
		[Inject] private readonly IAdvService _advService;
		[Inject] private readonly ITicketService _ticketService;
		[Inject] private readonly IItemUnlockService _unlockService;
		[Inject] private readonly ISettingsService _settingsService;

		private WorldSpaceSupplyBox _currentButton;
		private AdvWorldButtonPresenter _advPresenter;
		private TicketWorldButtonPresenter _ticketPresenter;
		private IDisposable _cooldown;
		private EquipmentItemData _equipmentItemData;

		protected override void OnStart()
		{
#if UNITY_EDITOR
			Debug.Assert(_storage is not null, $"[{nameof(EquipmentItemInteraction)}] storage is null on {name}");
#endif
			if (!_storage.All.TryGetValue(_equipmentDataId, out var itemData))
			{
				Debug.LogError("Can't find equipment data for " + _equipmentDataId);
				return;
			}
			_equipmentItemData = (EquipmentItemData)itemData;
			//Debug.Assert(_equipmentDataId is not null);

			if (_unlockService.IsUnlocked(_equipmentItemData))
			{
				Destroy(gameObject);
				return;
			}
			
			_advPresenter = new AdvWorldButtonPresenter(_advService, Callback, "Take supply:" + _equipmentItemData.Args.Id);
			_ticketPresenter = new TicketWorldButtonPresenter(_ticketService, Callback, "Take supply:" + _equipmentItemData.Args.Id);

			if (_outline)
			{
				_outline.Init();
				_outline.Enable();
			}
		}

		private void OnEnable()
		{
			if(_outline)
				_outline.Enable();
		}
		
		protected override void Select(CharacterContext characterContext)
		{
			var args = _equipmentItemData.Args;
			var controller = characterContext.EquipmentInventory.Controller;
			_equipmentInventoryService.AddNewEquipment(args);
			controller.ActiveEquipment.Select(args.PartType, args.Id);
			
			OnCooldown();
		}
		protected override void OnPlayerExit()
		{
			DisableUI();
		}
    
		protected override void OnPlayerEnter()
		{
			DisableUI();
			ShowUI();
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
			_currentButton.Count.gameObject.SetActive(false);
			_currentButton.Count.text = "";

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
