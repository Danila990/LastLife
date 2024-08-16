using System.Collections.Generic;
using System.Linq;
using Core.Inventory;
using Core.Inventory.Items;
using Core.Services.Input;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.InventoryUi
{
	public class InventoryPreviewView : UiView
	{
		public InventoryPreviewItem Prefab;
		public Transform Holder;
		public GameObject Content;
	}

	public class InventoryPreviewController : UiController<InventoryPreviewView>, IPostInitializable
	{
		private readonly IInventoryService _inventoryService;
		private readonly PlayerInputController _playerInputController;
		private readonly Dictionary<string, InventoryPreviewItem> _inventoryPreviewItems = new Dictionary<string, InventoryPreviewItem>();
		private InventoryPreviewItem SelectedItem { get; set; }
		public IReadOnlyDictionary<string, InventoryPreviewItem> Items => _inventoryPreviewItems;

		private bool _isDefaultPlayerInputRig;
		
		public InventoryPreviewController(IInventoryService inventoryService, PlayerInputController playerInputController)
		{
			_inventoryService = inventoryService;
			_playerInputController = playerInputController;
		}
		
		public void PostInitialize()
		{
			_playerInputController.ActiveInputRig.Subscribe(OnInputRigChanged).AddTo(View);
			_inventoryService.SelectedItem.Subscribe(OnItemChanged).AddTo(View);
			_inventoryService.InventoryChanged.Subscribe(InitializeFromInventory).AddTo(View);
			_inventoryService.ItemsRefreshed.Subscribe(OnEnabledChanged).AddTo(View);
			
			if (_inventoryPreviewItems.Count == 0)
			{
				SetActiveStatus(_isDefaultPlayerInputRig);
			}
		}
		
		private void OnEnabledChanged(Unit obj)
		{
			var countEnabled = 0;
			foreach (var (key, value) in _inventoryPreviewItems)
			{
				if (value.CurrentInvItem.IsEnabled)
				{
					countEnabled++;
					value.gameObject.SetActive(true);
				}
				else
				{
					value.gameObject.SetActive(false);
				}
			}
			SetActiveStatus(countEnabled > 0);
		}

		private void OnInputRigChanged(CustomInputRig rig)
		{
			if (rig is null)
				return;
			
			_isDefaultPlayerInputRig = rig.InputRigType is InputRigType.PlayerInputRig or InputRigType.MechInputRig;
			SetActiveStatus(_isDefaultPlayerInputRig);

			if (!_isDefaultPlayerInputRig)
				ClearSelectedItem();
				
			if(_isDefaultPlayerInputRig && _inventoryPreviewItems.Count == 1)
				OnItemClick(_inventoryPreviewItems.Values.First());
		}

		private void InitializeFromInventory(IInventory currentPlayerInventory)
		{
			DeleteAll();
			foreach (var inventoryItem in currentPlayerInventory.InventoryItems)
			{
				CreateItem(inventoryItem.ItemContext);
			}
			
			currentPlayerInventory.InventoryItems.ObserveAdd().Subscribe(OnItemAdded).AddTo(View);
			currentPlayerInventory.InventoryItems.ObserveRemove().Subscribe(OnItemRemoved).AddTo(View);
		}
		
		private void OnItemRemoved(CollectionRemoveEvent<ItemInvPair> obj)
		{
			DeleteItem(obj.Value.ItemContext);
		}

		private void OnItemAdded(CollectionAddEvent<ItemInvPair> obj)
		{
			CreateItem(obj.Value.ItemContext);
		}

		private void CreateItem(ItemContext invItem)
		{
			SetActiveStatus(_isDefaultPlayerInputRig);
			var newItem = Object.Instantiate(View.Prefab, View.Holder);
			newItem.Init(invItem);
			newItem.OnClick += OnItemClick;
			_inventoryPreviewItems.Add(invItem.ItemId, newItem);
		}
		
		private void DeleteItem(ItemContext invItem)
		{
			if(_inventoryPreviewItems.TryGetValue(invItem.ItemId, out var item))
			{
				ClearSelectedItem();

				Object.Destroy(item.gameObject);
				_inventoryPreviewItems.Remove(invItem.ItemId);
			}

			SetActiveStatus(_isDefaultPlayerInputRig);
		}

		private void DeleteAll()
		{
			foreach (var item in _inventoryPreviewItems)
			{
				Object.Destroy(item.Value.gameObject);
			}
			_inventoryPreviewItems.Clear();
		}
		
		private void OnItemChanged(ItemContext selectedItemContext)
		{
			if (selectedItemContext == null)
				return;

			if (SelectedItem && SelectedItem.CurrentInvItem == selectedItemContext)
				return;
			
			if (SelectedItem)
				SelectedItem.Deselect();
			foreach (var item in _inventoryPreviewItems)
			{
				if (item.Value.CurrentInvItem == selectedItemContext)
				{
					SelectedItem = item.Value;
					SelectedItem.Select();
					return;
				}
			}
		}
		
		private void OnItemClick(InventoryPreviewItem obj)
		{
			if (obj == SelectedItem)
				return;

			if (SelectedItem)
				SelectedItem.Deselect();
				
			SelectedItem = obj;
			_inventoryService.SelectItem(obj.CurrentInvItem);
			SelectedItem.Select();
		}

		private void SetActiveStatus(bool status)
		{
			var countItems = _inventoryService.CurrentPlayerInventory?.InventoryItems.Count(x => x.ItemContext.IsEnabled);
			if (!status || countItems is 0)
			{
				View.Content.SetActive(false);
				return;
			}

			View.Content.SetActive(_isDefaultPlayerInputRig /*&& countItems is not 0*/);
		}

		private void ClearSelectedItem()
		{
			if (!SelectedItem)
				return;
			
			SelectedItem.Deselect();
			SelectedItem = null;
		}
	}
}