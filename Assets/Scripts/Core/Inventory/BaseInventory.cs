using System;
using System.Linq;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Factory.Inventory;
using Core.Inventory.Items;
using Core.Inventory.Origins;
using Db.ObjectData;
using Db.ObjectData.Impl;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Inventory
{

	public class BaseInventory : MonoBehaviour, IInventory, IOriginProxy
	{
		[FoldoutGroup("Debug"), ShowInInspector] public ItemContext SelectedItem { get; private set; }
		[FoldoutGroup("Debug"), ShowInInspector] public EntityContext Owner { get; private set; }
		[FoldoutGroup("Debug"), ShowInInspector] private ReactiveCollection<ItemInvPair> _items;
		
		public Transform InventoryTransform;
		public InventoryObjectDataSo[] DefaultItems;
		public BaseOriginProvider OriginProvider;
		
		private ReactiveCommand<ItemContext> _onItemSelected;
		private ReactiveCommand _onItemDeselected;
		private ReactiveCommand _onEnabledItemsRefreshed;

		private ICameraService _cameraService;
		private IEntityRepository _entityRepository;
		private IInventoryItemsFactory _invItemsFactory;
		
		public BaseOriginProvider Origin => OriginProvider;
		public IReactiveCollection<ItemInvPair> InventoryItems => _items;
		public IObservable<ItemContext> OnItemSelected => _onItemSelected;
		public IObservable<Unit> OnItemDeselected => _onItemDeselected;
		public CompositeDisposable Disposable { get; set; }
		public IObservable<Unit> OnItemEnabledUpdated => _onEnabledItemsRefreshed;
		
		public void RefreshEnabledItems()
		{
			if (SelectedItem != null && SelectedItem.IsEnabled == false)
			{
				var item = InventoryItems.FirstOrDefault(item  => item.ItemContext.IsEnabled);
				SelectItem(item.ItemContext);
			}
			_onEnabledItemsRefreshed?.Execute();
		}

		public void SwitchInventoryActivity(bool status)
		{
			foreach (var item in _items)
			{
				item.ItemContext.SetEnabled(status);
			}
			RefreshEnabledItems();
		}
		
		#region Initialization
		
		public void Initialize(EntityContext owner, IObjectResolver resolver)
		{
			Owner = owner;
			_items = new ReactiveCollection<ItemInvPair>().AddTo(this);
			resolver.Inject(OriginProvider);
			_cameraService = resolver.Resolve<ICameraService>();
			_entityRepository = resolver.Resolve<IEntityRepository>();
			_invItemsFactory = resolver.Resolve<IInventoryItemsFactory>();
			
			foreach (var item in DefaultItems)
			{
				AddItem(item.Model);
			}
			
			_onItemSelected = new ReactiveCommand<ItemContext>().AddTo(this);
			_onItemDeselected = new ReactiveCommand().AddTo(this);
			_onEnabledItemsRefreshed = new ReactiveCommand().AddTo(this);
			_cameraService.IsThirdPersonObservable.Subscribe(OnChangeView).AddTo(this);
			Disposable = new CompositeDisposable();
		}

		public void OnAdapterSet(IEntityAdapter adapter)
		{
			foreach (var pair in _items)
			{
				pair.ItemContext.OnAdapterSet(adapter);
			}
		}

		#endregion

		#region Handling

		protected void LateUpdate()
		{
			if(!SelectedItem) 
				return;
			SelectedItem.InventoryUpdate();
		}

		#endregion

		#region Ineractions

		public void ForceRefresh()
		{
			_onItemSelected?.Execute(SelectedItem);
		}
		
		public void SelectItem(ItemContext newItem)
		{
			if(newItem == null) 
				return;
			
			if (SelectedItem)
				SelectedItem.OnDeselect();
			
			SelectedItem = newItem;
			SelectedItem.OnSelect();
			_onItemSelected?.Execute(newItem);
		}
		
		public bool TrySelectItem(string itemId)
		{
			var pair = InventoryItems.FirstOrDefault(item => item.Id == itemId);
			
			if (pair.ItemContext)
			{
				SelectItem(pair.ItemContext);
				return true;
			}
			return false;
		}
		
		public bool TrySelectItem(int itemIndex)
		{
			if (itemIndex >= _items.Count)
			{
				return false;
			}
			SelectItem(_items[itemIndex].ItemContext);
			return true;
		}

		public void InsertItem(InventoryObjectData inventoryObject, int index, int quantity = 0)
		{
			if (CheckContainsRefill(inventoryObject, quantity))
				return;

			var pair = CreateAndInitItem(inventoryObject, quantity);
			_items.Insert(index, pair);
		}

		public void UnSelect()
		{
			if (SelectedItem)
			{
				SelectedItem.OnDeselect();
				_onItemDeselected?.Execute();
			}
			
			SelectedItem = null;
		}
		
		public void RemoveItem(string itemToDel, bool selectNext = true)
		{
			var pair = _items.FirstOrDefault(invPair => invPair.Id == itemToDel);
			if (!pair.ItemContext)
				return;

			if(SelectedItem == pair.ItemContext)
				UnSelect();
			
			if (SelectedItem && SelectedItem.Equals(pair.ItemContext))
			{
				SelectedItem = null;
				pair.ItemContext.OnDeselect();
			}
			_items.Remove(pair);
			pair.ItemContext.OnDestroyed(_entityRepository);
			Destroy(pair.ItemContext.gameObject);
			
			var lastEnabled = _items.LastOrDefault(x => x.ItemContext.IsEnabled);
			if (_items.Count > 0 && selectNext && lastEnabled.ItemContext)
			{
				SelectItem(lastEnabled.ItemContext);
			}
		}

		public void AddItem(InventoryObjectData inventoryObject, int quantity = 0)
		{
			if (CheckContainsRefill(inventoryObject, quantity))
				return;
			
			var pair = CreateAndInitItem(inventoryObject, quantity);
			_items.Add(pair);
		}
		
		#endregion
		
		#region Private
		
		private ItemInvPair CreateAndInitItem(InventoryObjectData inventoryObject, int quantity)
		{
			var itemContext = CreateFromData(inventoryObject);
			itemContext.ItemInit(this);
			itemContext.Refill(quantity);
			return new ItemInvPair(itemContext, inventoryObject);
		}

		private ItemContext CreateFromData(InventoryObjectData inventoryObject)
		{
			return _invItemsFactory.Create(
				inventoryObject.InventoryObjectId, 
				InventoryTransform,
				Owner,
				inventoryObject.Id, 
				inventoryObject.Savable);
		}

		private void OnChangeView(bool status)
		{
			if (!SelectedItem) 
				return;
			SelectedItem.OnChangeView();
		}

		private bool CheckContainsRefill(IDataModel inventoryObject, int quantity)
		{
			var containedItem = _items.FirstOrDefault(x=> x.Id == inventoryObject.Id);
			if (containedItem.ItemContext)
			{
				containedItem.ItemContext.Refill(quantity);
				return true;
			}
			return false;
		}
		
		#endregion

		
	}

	public interface IInventory
	{
		IObservable<Unit> OnItemEnabledUpdated { get; }
		IReactiveCollection<ItemInvPair> InventoryItems { get; }
		ItemContext SelectedItem { get; }
		IObservable<ItemContext> OnItemSelected { get; }
		IObservable<Unit> OnItemDeselected { get; }
		CompositeDisposable Disposable { get; set; }
		void ForceRefresh();
		void SelectItem(ItemContext newItem);
		bool TrySelectItem(string itemId);
		bool TrySelectItem(int itemIndex);
		void UnSelect();
		void AddItem(InventoryObjectData inventoryObject, int quantity = 0);
		void RemoveItem(string contextToDel, bool selectNext = true);
	}

	public interface IOriginProxy
	{
		BaseOriginProvider Origin { get; }
	}

	public readonly struct ItemInvPair
	{
		public readonly ItemContext ItemContext;
		public readonly InventoryObjectData InventoryObject;
		public string Id => InventoryObject.Id;
		
		public ItemInvPair(ItemContext itemContext, InventoryObjectData inventoryObject)
		{
			ItemContext = itemContext;
			InventoryObject = inventoryObject;
		}
	}
}