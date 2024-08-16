using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.AiItem.Data
{
	[CreateAssetMenu(menuName = SoNames.AI_ITEM_DECORATORS + nameof(ThrowableWeaponAiItemContextedData), fileName = "ThrowableWeaponAiItemContextedData")]
	public class ThrowableWeaponAiItemContextedData : AiItemContextedData
	{

		public override AiItemContextDecorator CreateAiItem(ItemContext itemContext, EntityContext owner)
		{
			var aiItem = new ThrowableAiItemContext(itemContext, owner, this);
			aiItem.Created();
			//TODO: Move to ai item factory
			return aiItem;
		}
		
		public override bool IsApplicable(ItemContext itemContext)
		{
			if (itemContext is not ThrowableWeaponContext throwableWeaponContext)
				return false; 
			
			return true;
		}
	}
}