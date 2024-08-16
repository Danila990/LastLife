using System;
using Core.Entity.Repository;
using Core.Inventory;
using Db.ObjectData;
using Db.Panel;
using MessagePipe;
using Ui.Sandbox.SpawnMenu;
using Ui.Sandbox.SpawnMenu.Item;
using Ui.Widget;

namespace Core.Factory.Ui
{
	public interface ISpawnMenuItemFactory
	{
		ElementPresenterItem Create(ElementItemPanel element, OutlineButtonWidget selectElement, ObjectData objectData);
	}
	
	public class SpawnMenuItemFactory : ISpawnMenuItemFactory
	{
		private readonly ISpawnItemService _spawnItemService;
		private readonly IInventoryService _inventoryService;
		
		public SpawnMenuItemFactory(
			ISpawnItemService spawnItemService,
			IInventoryService inventoryService)
		{
			_spawnItemService = spawnItemService;
			_inventoryService = inventoryService;
		}

		public ElementPresenterItem Create(ElementItemPanel element, OutlineButtonWidget selectElement, ObjectData objectData)
		{
			ElementPresenterItem result;
			
			switch (element.ElementType)
			{
				case ElementType.InventoryAdd:
					result = new AddInventoryPresenterItem(element, selectElement, objectData, _inventoryService);
					break;
				case ElementType.ForSpawning:
					result = new SelectForSpawningPresenterItem(element, selectElement, objectData, _spawnItemService);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			return result;
		}
	}
}