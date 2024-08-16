using Core.Inventory;
using Core.Inventory.Items;
using UniRx;
using UnityEngine;

namespace Core.Entity.Ai.Sensor.InventorySensor
{
	public abstract class AbstractInventorySensor : MonoBehaviour
	{
		public IInventory Inventory { get; private set; }

		public void SetInv(IInventory inventory)
		{
			Inventory = inventory;
			OnInit();
		}

		public void ObserveInventory()
		{
			foreach (var invItem in Inventory.InventoryItems)
			{
				OnItemAdded(invItem.ItemContext);
			}
			
			Inventory.InventoryItems
				.ObserveAdd()
				.Subscribe(OnItemAddedInternal)
				.AddTo(this);

			Inventory.InventoryItems
				.ObserveRemove()
				.Subscribe(OnItemRemovedInternal)
				.AddTo(this);
		}

		protected virtual void OnInit() { }

		private void OnItemRemovedInternal(CollectionRemoveEvent<ItemInvPair> obj)
		{
			OnItemRemoved(obj.Value.ItemContext);
		}
		
		private void OnItemAddedInternal(CollectionAddEvent<ItemInvPair> obj)
		{
			OnItemAdded(obj.Value.ItemContext);
		}
		
		protected abstract void OnItemRemoved(ItemContext item);
		protected abstract void OnItemAdded(ItemContext item);
	}
}