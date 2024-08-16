using System;
using System.Linq;
using ControlFreak2;
using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.InputSystem;
using Core.Inventory;
using Core.Inventory.Items;
using Core.Services.Input;
using LitMotion;
using LitMotion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.PlayerInput
{
	public class PlayerInputView : UiView
	{
		public Image DotImg;
		public GameObject SwitchItemButton;
		public GameObject DistanceChange;
		public CustomInputRig CharInputRig;
		public CustomInputRig StaticItemRig;
		public CustomInputRig CarryingInputRig;
		public CustomInputRig MechInputRig;
		public SharedInput[] SharedInputs;
	}
	
	public class PlayerInputController : UiController<PlayerInputView>, IStartable, IDisposable
	{
		private readonly IInventoryService _inventoryService;
		private readonly IObjectResolver _resolver;
		private readonly IPlayerSpawnService _spawnService;
		private readonly ICameraService _cameraService;
		private readonly ReactiveProperty<CustomInputRig> _activeInputRig = new ReactiveProperty<CustomInputRig>();
		public IReadOnlyReactiveProperty<CustomInputRig> ActiveInputRig => _activeInputRig;
		private MotionHandle _handle;

		private IDisposable _disposable;
		private bool _isThirdPerson;
		
		public PlayerInputController(
			IInventoryService inventoryService, 
			IObjectResolver resolver,
			IPlayerSpawnService spawnService,
			ICameraService cameraService
		)
		{
			_inventoryService = inventoryService;
			_resolver = resolver;
			_spawnService = spawnService;
			_cameraService = cameraService;
		}
		
		public void Start()
		{
			_inventoryService.SelectedItem.SkipLatestValueOnSubscribe().Subscribe(OnSelectedItemChanged).AddTo(View);
			_inventoryService.ItemsRefreshed.Subscribe(OnEnabledCountChanged).AddTo(View);
			_spawnService.PlayerCharacterAdapter.ContextChanged.Subscribe(ContextChanged).AddTo(View);
			
			_spawnService.PlayerCharacterAdapter.AimController.TargetAim.SkipLatestValueOnSubscribe().Subscribe(status =>
			{
				if (_isThirdPerson)
					OnAim(status);
				
			}).AddTo(View);
			
			_cameraService.IsThirdPersonObservable.Subscribe(OnCameraChanged);
				
			View.CharInputRig.Init(_resolver);
			View.StaticItemRig.Init(_resolver);
			View.CarryingInputRig.Init(_resolver);

			SwitchInputRig(InputRigType.PlayerInputRig);
		}
		
		private void OnEnabledCountChanged(Unit obj)
		{
			UpdateSwitchActive();
		}
		
		private void UpdateSwitchActive()
		{
			var count = _inventoryService.CurrentPlayerInventory.InventoryItems.Count(x => x.ItemContext.IsEnabled);
		}

		private void ContextChanged(CharacterContext context)
		{
			_disposable?.Dispose();

			if (_activeInputRig.Value)
			{
				_activeInputRig.Value.OnContextChanged(context);
			}
		}

		public void SwitchInputRig(InputRigType rigType)
		{
			var newRig = rigType switch
			{
				InputRigType.PlayerInputRig => View.CharInputRig,
				InputRigType.StaticItemRig => View.StaticItemRig,
				InputRigType.CarryingInputRig => View.CarryingInputRig,
				InputRigType.MechInputRig => View.MechInputRig,
				_ => throw new ArgumentOutOfRangeException(nameof(rigType), rigType, null)
			};
			
			if (_activeInputRig.Value)
			{
				_activeInputRig.Value.Deactivate();
			}
		
			_activeInputRig.Value = newRig;
			_activeInputRig.Value.Activate();

				
			foreach (var sharedInput in View.SharedInputs)
			{
				if (newRig.SharedInputs.Contains(sharedInput.Id))
				{
					foreach (var i in sharedInput.Base)
					{
						i.gameObject.SetActive(true);
						i.SetRig(newRig.Rig);
						i.SetTouchControlPanel(newRig.TouchControlPanel);
					}
				}
				else
				{
					foreach (var i in sharedInput.Base)
					{
						i.gameObject.SetActive(false);
					}
				}
			}

			
			if (_activeInputRig.Value == View.CharInputRig || _activeInputRig.Value == View.MechInputRig)
			{
				var item = _inventoryService.SelectedItem.Value 
					? _inventoryService.SelectedItem.Value
					: _inventoryService.DefaultItem;
				
				_inventoryService.SelectItem(item);
				_activeInputRig.Value.OnSelectedItemChanged(item);
			}
		}

		public override void OnShow()
		{
			if (_activeInputRig.Value)
			{
				_activeInputRig.Value.Activate();
			}
		}

		public override void OnHide()
		{
			if (_activeInputRig.Value)
			{
				_activeInputRig.Value.Deactivate();
			}
		}

		private void OnCameraChanged(bool isThirdPerson)
		{
			_isThirdPerson = isThirdPerson;
			if (!isThirdPerson)
				OnAim(new AimStatus(AimState.Aim));
		}
		
		public void OnAim(AimStatus status)
		{
			_handle.IsActiveCancel();
			if (!View.gameObject.activeInHierarchy)
				return;
			
			if (status.State != AimState.Default)
			{
				_handle = LMotion
					.Create(View.DotImg.color.a, 1f, 0.1f)
					.BindToColorA(View.DotImg);
			}
			else
			{
				_handle = LMotion
					.Create(View.DotImg.color.a, 0f, 0.1f)
					.BindToColorA(View.DotImg);
			}
		}

		public void OnSelectedItemChanged(IItemActionsProvider selectedItem)
		{
			if (_activeInputRig.Value && _activeInputRig.Value == View.CharInputRig)
				_activeInputRig.Value.OnSelectedItemChanged(selectedItem);
			
			UpdateSwitchActive();
		}

		public void Dispose()
		{
			_handle.IsActiveCancel();
			_activeInputRig.Dispose();
			_disposable?.Dispose();
			
			View.CharInputRig.Dispose();
			View.StaticItemRig.Dispose();
			View.CarryingInputRig.Dispose();
		}
	}
	
	[Serializable]
	public class SharedInput
	{
		public string Id;
		public TouchControl[] Base;
	}
}