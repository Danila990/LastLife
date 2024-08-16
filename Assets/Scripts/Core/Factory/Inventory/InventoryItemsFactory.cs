using System.Collections.Generic;
using Core.Entity;
using Core.Factory.DataObjects;
using Core.Inventory.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Factory.Inventory
{
	public interface IInventoryItemsFactory
	{
		ItemContext Create(string invObjId, Transform parent, EntityContext owner, string itemId, bool savable);
	}
	
	public class InventoryItemsFactory : IInitializable, IInventoryItemsFactory
	{
		private readonly IFactoryData _factoryData;
		private readonly IObjectResolver _resolver;
		
		private readonly Dictionary<string, ItemContext> _inventoryItemsFactory = new Dictionary<string, ItemContext>();
		
		public InventoryItemsFactory(IFactoryData factoryData, IObjectResolver resolver)
		{
			_factoryData = factoryData;
			_resolver = resolver;
		}
		
		public void Initialize()
		{
			foreach (var factoryDataObject in _factoryData.Objects)
			{
				if (factoryDataObject.Type == EntityType.InventoryItem)
				{
					_inventoryItemsFactory.Add(factoryDataObject.Key, (ItemContext)factoryDataObject.Object);
				}
			}
		}

		public ItemContext Create(string invObjId, Transform parent, EntityContext owner, string itemId, bool savable)
		{
			var item = _inventoryItemsFactory[invObjId];
			var newItem =  Object.Instantiate(item, parent);
			
			newItem.ItemId = itemId;
			newItem.Savable = savable;
			newItem.Owner = owner;
			newItem.Uid = ObjectFactory.GetNextUid();
			
			_resolver.Inject(newItem);
			newItem.Created(_resolver,invObjId);
			
			return newItem;
		}
	}
}