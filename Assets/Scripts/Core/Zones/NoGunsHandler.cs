using System.Linq;
using Core.Boosts.Entity;
using Core.Entity.Characters;
using Core.Inventory.Items.Weapon;
using Db.ObjectData;

namespace Core.Zones
{
	public class NoGunsHandler : ZoneHandler
	{
		private InventoryObjectData[] _cachedData;

		protected override void OnHandle(in ZoneChangedMessage zoneMsg)
		{
			var inventory = zoneMsg.Context.Inventory;

			foreach (var inventoryObjectData in inventory.InventoryItems
				         .Where(x => x.ItemContext is not MeleeWeaponContext && x.ItemContext is not BoostEntity))
			{
				inventoryObjectData.ItemContext.SetEnabled(false);
			}

			inventory.RefreshEnabledItems();
		}

		protected override void OnResetContext(CharacterContext context)
		{
			var inventory = context.Inventory;
			foreach (var inventoryObjectData in inventory.InventoryItems)
			{
				inventoryObjectData.ItemContext.SetEnabled(true);
			}
			
			inventory.RefreshEnabledItems();
		}
		
		protected override void OnResetSelf()
		{
			
		}
	}
}
