using System;
using System.Collections.Generic;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Shop.Models.Product
{
	[Serializable]
	public struct InAppPrice
	{
		public string ISO;
		public float Price;

		public InAppPrice(string iso, float price)
		{
			ISO = iso;
			Price = price;
		}

		public override string ToString()
		{
			return $"{ISO} {Price.ToString(CultureInfo.InvariantCulture)}";
		}
		
	}
	
	[Serializable]
	public class InAppProduct
	{
		public string InAppID;
		public string Name;
		public InAppPrice Price;
		[GUIColor("GetColor")]
		public ProductType ProductType;
		[TableList]
		public StoreSpecificIds[] SpecificIds;


		private Color GetColor()
		{
			switch (ProductType)
			{
				case ProductType.Consumable:
					return Color.green;
				case ProductType.NonConsumable:
					return Color.white;
				case ProductType.Subscription:
					return Color.cyan;
			}
			return Color.black;
		}
		
		
		[Serializable]
		public struct StoreSpecificIds
		{
			public string SpecificId;
			[ValueDropdown("StoreIds")]
			[ListDrawerSettings(ShowFoldout = false)]
			public string[] StoresIds;
			
#if UNITY_EDITOR
			public static IEnumerable<string> StoreIds = new[]
			{
				GooglePlay.Name,
				AppleAppStore.Name
			};
#endif
		}
	}
}