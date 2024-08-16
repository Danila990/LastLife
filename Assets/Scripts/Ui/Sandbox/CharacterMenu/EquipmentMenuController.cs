using System;
using System.Collections.Generic;
using System.Linq;
using Core.Equipment;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Equipment.Inventory;
using Core.Services;
using Ui.Widget;

namespace Ui.Sandbox.CharacterMenu
{
	public class EquipmentMenuController : IDisposable
	{
		private readonly EquipmentMenuData _menuData;
		private readonly Dictionary<EquipmentPartType, EquipmentPresenter> _presenters;

		private EquipmentInventory _inventory;
		private EquipmentPresenter _activePresenter;
		private readonly IItemStorage _itemStorage;

		public EquipmentMenuController(EquipmentMenuData menuData, IItemStorage itemStorage)
		{
			_itemStorage = itemStorage;
			_presenters = new();
			_menuData = menuData;
			AddButtonsCallback();
		}
		
		public void Dispose()
		{
			foreach (var presenter in _presenters.Values)
			{
				presenter?.Dispose();
			}
			_activePresenter?.Dispose();
		}

		public void SetInventoryRef(EquipmentInventory inventory)
		{
			_inventory = inventory;
			foreach (var presenter in _presenters.Values)
				presenter.Dispose();
			
			_presenters.Clear();
			
			foreach (var widget in _menuData.Widgets)
			{
				_presenters[widget.Type] = new EquipmentPresenter(_inventory, widget, _itemStorage, _menuData, HideContent);
			}
		}

		private void AddButtonsCallback()
		{
			foreach (var widget in _menuData.Widgets)
			{
				widget.Button.onClick.AddListener(() => ShowContent(widget));
				widget.Prewarm();
			}
			
			_menuData.HideButton.onClick.AddListener(HideContent);
			_menuData.SkillsButtons.onClick.AddListener(HideContent);
			_menuData.CrossButton.onClick.AddListener(HideContent);
		}

		private void ShowContent(EquipmentButtonWidget widget)
		{
			_menuData.HideButton.gameObject.SetActive(true);
			_activePresenter?.Hide();
			_activePresenter = _presenters[widget.Type];
			_activePresenter.Show();
		}

		public void Show()
		{
			Refresh();
			_menuData.Equipment.SetActive(true);
		}

		public void Hide()
		{
			_menuData.Equipment.SetActive(false);
		}

		private void Refresh()
		{
			foreach (var widget in _menuData.Widgets)
			{
				if (_inventory.Controller.ActiveEquipment.TryGetActivePart<EquipmentEntityContext>(widget.Type, out var context))
				{
					if (_itemStorage.EquipmentByType.TryGetValue(context.PartType, out var dataList))
					{
						var itemData = dataList.FirstOrDefault(x => x.Args.Id == context.GetItemArgs().Id);

						if (itemData != null)
						{
							widget.IconImg.sprite = itemData.Ico;
							widget.IconImg.enabled = true;
							continue;
						}
						
					}
				}
				widget.BGImg.sprite = _menuData.PlusSprite;
				widget.IconImg.enabled = false;
			}
		}
		
		public void HideContent()
		{
			_menuData.HideButton.gameObject.SetActive(false);
			_activePresenter?.Hide();
		}
		
	}

}
