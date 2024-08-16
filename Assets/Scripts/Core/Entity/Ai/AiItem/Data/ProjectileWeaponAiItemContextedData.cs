using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.AiItem.Data
{
	[CreateAssetMenu(menuName = SoNames.AI_ITEM_DECORATORS + nameof(ProjectileWeaponAiItemContextedData), fileName = "ProjectileWeaponAiItemData")]
	public class ProjectileWeaponAiItemContextedData : AiItemContextedData
	{
		[field:TitleGroup("Factory Data Routing")]
		[field:SerializeField] public bool UniqueWeapon { get; set; }
		[field:SerializeField] public string ItemId { get; set; }

		public override AiItemContextDecorator CreateAiItem(ItemContext itemContext, EntityContext owner)
		{
			var aiItem = new ShootingAiItemContext((ProjectileWeaponContext)itemContext, owner, this);
			aiItem.Created();
			//TODO: Move to ai item factory
			return aiItem;
		}
		
		public override bool IsApplicable(ItemContext itemContext)
		{
			if (itemContext is not ProjectileWeaponContext projectileWeaponContext)
				return false; 
			
			
			if (UniqueWeapon)
			{
				//TODO: ADD Getting Special weapon settings from storage
				return ItemId == itemContext.ItemId;
			}
			
			return true;
		}
	}
}