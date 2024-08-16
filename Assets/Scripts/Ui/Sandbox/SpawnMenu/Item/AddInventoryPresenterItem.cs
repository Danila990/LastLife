using Core.Inventory;
using Db.ObjectData;
using Db.Panel;
using Ui.Widget;
using UniRx;

namespace Ui.Sandbox.SpawnMenu.Item
{

	public class AddInventoryPresenterItem : ElementPresenterItem
	{
		private readonly IInventoryService _inventoryService;
		public override string Id => ElementData.ItemObjectDataId;
		private readonly InventoryObjectData _inventoryObject;
		
		public AddInventoryPresenterItem(
			ElementItemPanel elementData,
			ButtonWidget widget,
			ObjectData objectData,
			IInventoryService inventoryService) : base(elementData, widget, objectData)
		{
			_inventoryService = inventoryService;
			_inventoryObject = objectData as InventoryObjectData;
		}

		public override void SetBlocked(bool status)
		{
			
		}
		protected override void OnClickButton(Unit obj)
		{
			_inventoryService.TryAddItem(_inventoryObject);
		}
	}
}