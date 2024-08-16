using System;
using Core.ResourcesSystem;
using Sirenix.OdinInspector;

namespace Db.MerchantData
{
	[Serializable]
	public class MerchantShopItemData : ObjectData.ObjectData
	{
		public bool TransparentIcon;
		public bool DrawPrice;
		
		[HorizontalGroup("Price")] public ResourceType ResourceType;
		[HorizontalGroup("Price")] public int Price;

		[EnumToggleButtons]
		public StoreItemType StoreItemType;
		
		[ValueDropdown("@ObjectsData.AllIds")]
		[InlineButton("@ObjectsData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string ObjectDataIdToBuy;
	}

	public enum StoreItemType
	{
		Consumable = 0,
		NonConsumable = 1,
		Custom = 10
	}
}