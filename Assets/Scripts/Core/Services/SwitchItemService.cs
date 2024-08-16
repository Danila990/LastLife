using System;
using System.Linq;
using Core.Inventory;
using Core.Services.Input;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using Utils;
using Utils.Constants;
using VContainer.Unity;

namespace Core.Services
{
	public class SwitchItemService : IInitializable, IDisposable
	{
		private readonly IInventoryService _inventoryService;
		private readonly IInputService _inputService;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private IInventory _currentInv;

		public SwitchItemService(IInventoryService inventoryService, IInputService inputService)
		{
			_inventoryService = inventoryService;
			_inputService = inputService;
		}
		
		public void Initialize()
		{
			_inventoryService.InventoryChanged.Subscribe(SetInv).AddTo(_compositeDisposable);
			_inputService.ObserveGetButtonDown(InputConsts.SWITCH_WEAPON).Subscribe(OnButtonDown).AddTo(_compositeDisposable);
		}

		public void SwitchItem()
		{
			if (_currentInv == null)
				return;
			
			if (_inventoryService.SelectedItem.Value == null && 0.InBounds(_currentInv.InventoryItems))
			{
				_inventoryService.SelectItem(_currentInv.InventoryItems[0].ItemContext);
				return;
			}
				
			var selectedItem = _inventoryService.SelectedItem.Value;
			var selectedIndex = _inventoryService.CurrentPlayerInventory.InventoryItems.IndexOf(item => item.Id == selectedItem.ItemId);
			var count = _inventoryService.CurrentPlayerInventory.InventoryItems.Count(x => x.ItemContext.IsEnabled);
			if (count <= 1)
				return;
			
			var nextItemIndex = 0;
			
			for (var index = selectedIndex; index < _currentInv.InventoryItems.Count; index++)
			{
				var item = _currentInv.InventoryItems[index];

				if (item.Id == selectedItem.ItemId || !item.ItemContext.IsEnabled)
				{
					if (index + 1 == _currentInv.InventoryItems.Count)
					{
						index = -1;
					}
					continue;
				}
				nextItemIndex = index;
				break;
			}
			
			if(nextItemIndex.InBounds(_currentInv.InventoryItems))
				_inventoryService.SelectItem(_currentInv.InventoryItems[nextItemIndex].ItemContext);
		}
		
		private void OnButtonDown(Unit obj)
		{
			if (_inventoryService.CurrentContext&& !_inventoryService.CurrentContext.Health.IsDeath)
			{
				SwitchItem();
			}
		}

		private void SetInv(IInventory obj)
		{
			_currentInv = obj;
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}