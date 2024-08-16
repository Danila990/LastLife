using System.Linq;
using Core.Entity.Characters;
using Core.Inventory.Items;
using Db.ObjectData;
using UnityEngine;
using Utils.Constants;

namespace Core.Zones
{
	public class NonGravGunHandler : ZoneHandler
	{
		
		private (InventoryObjectData Item, int Index) _cachedData;

		protected override void OnHandle(in ZoneChangedMessage zoneMsg)
		{
			var inventory = zoneMsg.Context.Inventory;
			var foundContext = inventory.InventoryItems.FirstOrDefault(x => x.ItemContext is GravyGun);
			if (foundContext.ItemContext != null)
			{
				_cachedData.Item = foundContext.InventoryObject;
				_cachedData.Index = inventory.InventoryItems.IndexOf(foundContext);
				if(inventory.SelectedItem == foundContext.ItemContext)
					inventory.UnSelect();
				
				inventory.RemoveItem(foundContext.ItemContext.ItemId);
			}
			
		}

		protected override void OnResetContext(CharacterContext context)
		{
			if(_cachedData.Item == null)
				return;
			
			var inventory = context.Inventory;
			inventory.InsertItem(_cachedData.Item, _cachedData.Index);
		}
		
		protected override void OnResetSelf()
		{
			_cachedData.Item = null;
			_cachedData.Index = -1;
		}
	}

}
