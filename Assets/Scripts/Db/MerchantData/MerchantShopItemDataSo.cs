using Db.ObjectData.Impl;
using UnityEngine;
using Utils;

namespace Db.MerchantData
{
	[CreateAssetMenu(menuName = SoNames.MERCHANT_ITEMS + nameof(MerchantShopItemDataSo), fileName = "MerchantShopItemData")]
	public class MerchantShopItemDataSo : ObjectDataSo<MerchantShopItemData>
	{

		public override void EditorSet(MerchantShopItemData model)
		{
			base.EditorSet(model);
			Model.ObjectDataIdToBuy = model.ObjectDataIdToBuy;
			Model.ResourceType = model.ResourceType;
			Model.StoreItemType = model.StoreItemType;
			Model.Price = model.Price;
		}
	}
}