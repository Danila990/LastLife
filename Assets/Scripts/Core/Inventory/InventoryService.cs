using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.Characters;
using Core.InputSystem;
using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using Core.Services;
using Cysharp.Threading.Tasks;
using Db.ObjectData;
using MessagePipe;
using SharedUtils;
using UniRx;
using VContainer.Unity;

namespace Core.Inventory
{
	public class InventoryService : IInventoryService, IStartable, IDisposable
	{
		private readonly ISubscriber<PlayerContextChangedMessage> _contextSub;
		private readonly IItemStorage _itemStorage;
		private readonly IInventorySaveAdapter _saveAdapter;
		private readonly ReactiveCommand<IInventory> _inventoryChanged;
		private readonly ReactiveProperty<ItemContext> _selectedItem;
		private readonly ReactiveCommand _itemsRefreshed;
		private IInventory _currentPlayerInventory;
		public IInventory CurrentPlayerInventory => _currentPlayerInventory;
		public IObservable<IInventory> InventoryChanged => _inventoryChanged;
		public IObservable<Unit> ItemsRefreshed => _itemsRefreshed;
		public IReadOnlyReactiveProperty<ItemContext> SelectedItem => _selectedItem;
		public ItemContext DefaultItem => GetDefaultItem();
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private int _lastSelectedItemIndex = -1;
		private CompositeDisposable _disposable;
		private IDisposable _quantityObserving;

		
#if UNITY_INCLUDE_TESTS
		public int ToSaveTest { get; private set; }
#endif
		
		public CharacterContext CurrentContext { get; private set; }

		public InventoryService(
			ISubscriber<PlayerContextChangedMessage> contextSub,
			IItemStorage itemStorage,
			IInventorySaveAdapter saveAdapter
			)
		{
			_contextSub = contextSub;
			_itemStorage = itemStorage;
			_saveAdapter = saveAdapter;
			_inventoryChanged = new ReactiveCommand<IInventory>();
			_selectedItem = new ReactiveProperty<ItemContext>();
			_itemsRefreshed = new ReactiveCommand();
		}
		
		public void Start()
		{
			_contextSub.
				Subscribe(Handler)
				.AddTo(_compositeDisposable);
		}

		private void Handler(PlayerContextChangedMessage msg)
		{
			CurrentContext = msg.CharacterContext;

			if (msg.Created)
			{
				RestoreItems(in msg);
				
				msg.CharacterContext
					.Inventory
					.OnItemEnabledUpdated
					.Subscribe(unit => _itemsRefreshed.Execute())
					.AddTo(msg.CharacterContext.destroyCancellationToken);
			}
		}

		private ItemContext GetDefaultItem()
		{
			return _currentPlayerInventory?.InventoryItems.FirstOrDefault(x => x.ItemContext is MeleeWeaponContext).ItemContext;
		}

		public void SetInventory(IInventory inventory)
		{
			UnBind();
			_currentPlayerInventory?.SelectItem(_currentPlayerInventory.InventoryItems[0].ItemContext);
			_currentPlayerInventory = inventory;
			_currentPlayerInventory.OnItemSelected.Subscribe(OnItemSelected).AddTo(_currentPlayerInventory.Disposable);
			_currentPlayerInventory.OnItemDeselected.Subscribe(OnItemDeselected).AddTo(_currentPlayerInventory.Disposable);
			_inventoryChanged.Execute(_currentPlayerInventory);
			_itemsRefreshed?.Execute();
			DelaySelect(0).Forget();
		}

		private void UnBind()
		{
			if(_currentPlayerInventory is null) return;
			_currentPlayerInventory.Disposable?.Dispose();
			_currentPlayerInventory.Disposable = new CompositeDisposable();
		}
		
		private void RestoreItems(in PlayerContextChangedMessage msg)
		{
			_disposable?.Dispose();
			_disposable = new CompositeDisposable();
			UnBind();
			_currentPlayerInventory = msg.CharacterContext.Inventory;
			_currentPlayerInventory.OnItemSelected.Subscribe(OnItemSelected).AddTo(_currentPlayerInventory.Disposable);
			_currentPlayerInventory.OnItemDeselected.Subscribe(OnItemDeselected).AddTo(_currentPlayerInventory.Disposable);

			if (_saveAdapter.TryGetData(out var savedItems))
			{
				foreach (var item in savedItems.Data)
				{
					if(_itemStorage.InventoryItems.TryGetValue(item.Key, out var data))
						_currentPlayerInventory.AddItem(data, item.Value);
				}
			}

			_inventoryChanged.Execute(_currentPlayerInventory);
			DelaySelect(_lastSelectedItemIndex).Forget();
		}
		
		private void OnItemSelected(ItemContext itemContext)
		{
			if(itemContext == null)
				return;

			if (itemContext == _selectedItem.Value)
			{
				_selectedItem.SetValueAndForceNotify(itemContext);
				return;
			}
			
			_quantityObserving?.Dispose();
			if (itemContext.HasQuantity)
				_quantityObserving = itemContext.CurrentQuantity.Subscribe(_ => OnQuantityChanged());
			
			_selectedItem.Value = itemContext;
			_lastSelectedItemIndex = _currentPlayerInventory.InventoryItems.IndexOf(x => x.ItemContext.ItemId == itemContext.ItemId);
		}
		
		private void OnItemDeselected(Unit _)
		{
			_selectedItem.Value = null;
			_lastSelectedItemIndex = -1;
		}

		private void OnQuantityChanged()
		{
			if(_selectedItem is {Value: null})
				return;
			
			_saveAdapter.Refresh(_selectedItem.Value, _selectedItem.Value.CurrentQuantity.Value);
		}
		
		private async UniTaskVoid DelaySelect(int itemIndex)
		{
			await UniTask.NextFrame(); //TODO CHANGE CALL STACK
			if (itemIndex >= 0 && _currentPlayerInventory.TrySelectItem(itemIndex))
			{
			}
			else
			{
				_currentPlayerInventory.SelectItem(_currentPlayerInventory.InventoryItems.FirstOrDefault(context => context.ItemContext is GravyGun).ItemContext);
			}
		}
		
		public void SelectItem(ItemContext itemContext)
		{
			_currentPlayerInventory?.SelectItem(itemContext);
		}
		
		public void UnselectItem()
		{
			_currentPlayerInventory.UnSelect();
			_selectedItem.Value = null;
		}

		public void RemoveItem(ItemContext context)
		{
			_currentPlayerInventory.RemoveItem(context.ItemId);
		}
		
		public void TryAddItem(InventoryObjectData inventoryObject)
		{
			_currentPlayerInventory?.AddItem(inventoryObject);
		}

		public void Dispose()
		{
			_itemsRefreshed?.Dispose();
			_inventoryChanged?.Dispose();
			_selectedItem?.Dispose();
			_quantityObserving?.Dispose();
			_compositeDisposable.Dispose();
		}
	}

	public interface IInventoryService
	{
		IObservable<IInventory> InventoryChanged { get; }
		IObservable<Unit> ItemsRefreshed { get; }
		CharacterContext CurrentContext { get; }
		IReadOnlyReactiveProperty<ItemContext> SelectedItem { get; }
		public IInventory CurrentPlayerInventory { get; }
		void SelectItem(ItemContext itemContext);
		void UnselectItem();
		ItemContext DefaultItem { get; }
		void TryAddItem(InventoryObjectData inventoryObject);
		public void SetInventory(IInventory inventory);
		
#if UNITY_INCLUDE_TESTS
		public int ToSaveTest { get; }
#endif
	}

}