using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.AiItem.Data
{
	[CreateAssetMenu(menuName = SoNames.AI_ITEM_DECORATORS + nameof(SpawnAiItemContextData), fileName = "SpawnAiItemContextData")]
	public class SpawnAiItemContextData : AiItemContextedData
	{
		public override AiItemContextDecorator CreateAiItem(ItemContext itemContext, EntityContext owner)
		{
			var aiItem = new SpawnAiItemContext((SpawnLifeEntityWeaponContext)itemContext, owner, this);
			aiItem.Created();
			//TODO: Move to ai item factory
			return aiItem;
		}
		
		public override bool IsApplicable(ItemContext itemContext)
		{
			return itemContext is SpawnLifeEntityWeaponContext;
		}
	}
}