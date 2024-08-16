using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shop.Models
{
	[Serializable]
	public class BundleShopItemModel : ShopItemModel
	{
		[ValueDropdown("@AInAppDatabase.AllIds")]
		public string[] ShopItemIds;
		
		[Range(1, 5)]
		public float DiscountMlp;
	}
}