using Core.Entity.Characters;
using Db.ObjectData.Impl;
using UnityEngine;

namespace Core.Loot
{
	public class ItemLootEntity : LootEntity
	{
		[SerializeField] private InventoryObjectDataSo _objectToGive;
		[SerializeField] private int _quantity;

		#if UNITY_INCLUDE_TESTS
		public InventoryObjectDataSo ObjectToGiveTest => _objectToGive;
		#endif
		
		protected override void OnInteractWithPlayer(CharacterContext context)
		{
			if(_quantity < 0)
				return;
			
			context.Inventory.AddItem(_objectToGive.Model, _quantity);
		}
	}
}
