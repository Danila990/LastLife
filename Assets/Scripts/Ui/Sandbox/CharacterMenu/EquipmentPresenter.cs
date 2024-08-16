using System.Linq;
using System;
using Core.Equipment;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Equipment.Inventory;
using Core.Services;
using Db.ObjectData;
using Ui.Widget;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.CharacterMenu
{
	public class EquipmentPresenter : IDisposable
	{
		private readonly EquipmentInventory _inventory;
		private readonly EquipmentButtonWidget _widget;
		private readonly IItemStorage _storage;
		private readonly EquipmentMenuData _menuData;
		private readonly Action _onClick;
		private readonly CompositeDisposable _disposable;
		
		private IEquipmentArgs _activeArgs;

		private OutlineButtonWidget _selectedWidget;
		private ButtonWidget _deselectButton;

		public EquipmentPresenter(EquipmentInventory inventory,
			EquipmentButtonWidget widget,
			IItemStorage storage,
			EquipmentMenuData menuData,
			Action onClick)
		{
			_inventory = inventory;
			_widget = widget;
			_storage = storage;
			_menuData = menuData;
			_onClick = onClick;
			
			_disposable = new CompositeDisposable();
			_inventory.Controller.ActiveEquipment.OnUnequip.Subscribe(args => OnUnequip(args.Context)).AddTo(_disposable);
			_inventory.Controller.ActiveEquipment.OnEquip.Subscribe(OnEquip).AddTo(_disposable);
			CreateDeselectButton();
		}

		public void Dispose()
		{
			_disposable?.Dispose();

			if(_deselectButton)
				Object.Destroy(_deselectButton.gameObject);
			
			if (_selectedWidget != null)
				_selectedWidget.DeselectOutline();
		}

		private void CreateDeselectButton()
		{
			_deselectButton = Object.Instantiate(_menuData.DeselectButton, _widget.Container.transform);
			_deselectButton.transform.SetSiblingIndex(0);
			_deselectButton.Button
				.OnClickAsObservable()
				.Subscribe(_ => Deselect())
				.AddTo(_disposable);
		}

		private void Deselect()
		{
			_onClick();

			if(_activeArgs == null)
				return;

			var unequipArgs = new UnequipArgs(UnequipReason.ByPlayer, _activeArgs.PartType);
			_inventory.Controller.ActiveEquipment.Deselect(ref unequipArgs);
			
			if (_selectedWidget != null)
				_selectedWidget.DeselectOutline();
			
			_selectedWidget = null;
		}
		
		private void Select(IEquipmentArgs args, OutlineButtonWidget widget)
		{
			if(widget == _selectedWidget)
				return;
			
			if(!_inventory.Controller.AllEquipment.HasEquipment(args.PartType, args.Id))
				return;
			
			SwitchSelected(widget, args);
			_inventory.Controller.ActiveEquipment.Select(args.PartType, args.Id);
			_onClick();
		}
		
		private void OnUnequip(EquipmentEntityContext context)
		{
			if (_activeArgs == null)
				return;

			if (_activeArgs.Id == context.GetItemArgs().Id)
			{
				_widget.IconImg.sprite = null;
				_widget.IconImg.enabled = false;
			}
		}
		
		private void OnEquip(EquipmentEntityContext context)
		{
			if(_widget.Type != context.PartType)
				return;

			_activeArgs = context.GetItemArgs();

			var itemData = _storage.EquipmentByType[_widget.Type].First(x => x.Args.Id == _activeArgs.Id);
			_widget.IconImg.sprite = itemData.Ico;
			_widget.IconImg.enabled = true;
			
		}
		
		public void Show()
		{
			_widget.Container.SetActive(true);
			if (!_inventory.Controller.AllEquipment.EquipmentByType.TryGetValue(_widget.Type, out var items))
				return;

			var itemData = _storage.EquipmentByType[_widget.Type];
			var siblingIndex = 0;
			foreach (var item in items.Values)
			{
				var foundData = itemData.FirstOrDefault(x => x.Args.Id == item.EquipmentArgs.Id);
				if (foundData != null)
				{
					CreateWidgetByItem(item, foundData, siblingIndex);
					siblingIndex++;
				}
			}
		}

		public void Hide()
		{
			_widget.Container.SetActive(false);
			_widget.Pool.ReturnAll();
		}

		private void CreateWidgetByItem(in RuntimeEquipmentArgs runtimeArgs, ObjectData itemData, int siblingIndex)
		{
			var pooled = _widget.Pool.Rent();
			pooled.IconImg.sprite = itemData.Ico;
			var equipmentArgs = runtimeArgs.EquipmentArgs; 
			pooled.Button.onClick.AddListener(() => Select(equipmentArgs, pooled));

			if (runtimeArgs.IsBlocked)
			{
				pooled.DeselectAlphaForIcon(0f, 0.5f);
				pooled.Button.interactable = false;
			}
			
			if (_inventory.Controller.ActiveEquipment.Equipment.TryGetValue(equipmentArgs.PartType, out var context))
			{
				if (equipmentArgs.Id == context.GetItemArgs().Id)
					SwitchSelected(pooled, equipmentArgs);
			}
			
			pooled.transform.SetSiblingIndex(siblingIndex + 1);
			pooled.gameObject.SetActive(true);
		}
		
		private void SwitchSelected(OutlineButtonWidget widget, IEquipmentArgs args)
		{
			if (_selectedWidget != null)
				_selectedWidget.DeselectOutline();

			_selectedWidget = widget;
			_activeArgs = args;
			_widget.IconImg.sprite = widget.IconImg.sprite;
			_widget.IconImg.enabled = true;
			_selectedWidget.SelectOutline();
		}
	}
}
