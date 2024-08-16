using System;
using ControlFreak2;
using Core.Actions;
using Core.Entity.Characters;
using Core.Inventory.Items;
using Sirenix.OdinInspector;
using TMPro;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Services.Input
{
	public class CustomInputRig : MonoBehaviour, IDisposable
	{
		[field:SerializeField] public InputRigType InputRigType { get; private set; }
		[SerializeField] private InputButtonElement[] _inputButtons;
		[SerializeField] private InputRig _rig;
		[SerializeField] private TouchControlPanel _touchControlPanel;
		[SerializeField] private GameObject _quantityInfoGameObject;
		[SerializeField] private TextMeshProUGUI _quantityInfoText;
		
		public string[] SharedInputs;
		public bool RigIsActive { get; private set; }
		public InputRig Rig => _rig;
		public TouchControlPanel TouchControlPanel => _touchControlPanel;
		private IDisposable _runtimeDisposable;
		private IItemActionsProvider _selectedItem;
		

		public void Init(IObjectResolver resolver)
		{
			foreach (var inputButton in _inputButtons)
			{
				resolver.Inject(inputButton);
				inputButton.Init();
			}
			
			Deactivate();
		}
		
		public void OnContextChanged(CharacterContext context)
		{
			foreach (var inputButtonElement in _inputButtons)
			{
				inputButtonElement.OnContextChanged(context);
			}
		}
		
		public void OnSelectedItemChanged(IItemActionsProvider selectedItem)
		{
			HideAllButtons();
			if (selectedItem == null)
			{
				_quantityInfoGameObject.SetActive(false);
				return;
			}

			_selectedItem = selectedItem;
			_quantityInfoGameObject.SetActive(_selectedItem.HasQuantity && RigIsActive);
			_runtimeDisposable?.Dispose();
			_runtimeDisposable = selectedItem.CurrentQuantity.Subscribe(OnQuantityChange);
			
			foreach (var action in selectedItem.ActionProvider.ActionControllers)
			{
				if (GetButtonByAction(action.ActionKey, out var element))
				{
					element.SetUp(action);
				}
				else
				{
					//Debug.LogError("Not Binded");
				}
			}
		}

		public void Activate()
		{
			RigIsActive = true;
			gameObject.SetActive(true);
			CF2Input.activeRig = _rig;
			if (_selectedItem != null)
				_quantityInfoGameObject.SetActive(_selectedItem.HasQuantity && RigIsActive);
		}

		public void Deactivate()
		{
			RigIsActive = false;
			foreach (var btn in _inputButtons)
				btn.Disable();

			_rig.ResetState();
			_quantityInfoGameObject.SetActive(false);
			gameObject.SetActive(false);
		}
		
		private void OnQuantityChange(int amount)
		{
			_quantityInfoText.text = amount.ToString();
		}
		
		private void HideAllButtons()
		{
			foreach (var button in _inputButtons)
			{
				button.Hide();
			}
		}

		private bool GetButtonByAction(ActionKey actionKey, out InputButtonElement element)
		{
			foreach (var inputButton in _inputButtons)
			{
				if (inputButton.ButtonKey != actionKey)
					continue;
				element = inputButton;
				return true;
			}
			element = null;
			return false;
		}
		
		public void Dispose()
		{
			_runtimeDisposable?.Dispose();
		}

#if UNITY_EDITOR
		[Button]
		public void GetAllInputButtons()
		{
			_inputButtons = transform.GetComponentsInChildren<InputButtonElement>();
		}
#endif
	}
	
	public enum InputRigType
	{
		PlayerInputRig,
		StaticItemRig,
		CarryingInputRig,
		MechInputRig,
	}
}