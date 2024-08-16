using System.Collections.Generic;
using Db.ObjectData;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db.MerchantData
{
	public interface IMerchantItemCollectionData : IDataModel
	{
		IReadOnlyList<MerchantShopItemDataSo> Items { get; }
	}
	
	[CreateAssetMenu(menuName = SoNames.MERCHANT_ITEMS + "Collection", fileName = "MerchantItemCollectionData")]
	public class MerchantItemCollectionData : ScriptableObject, IMerchantItemCollectionData
	{
		[field:SerializeField, HorizontalGroup("Id")] public string Id { get; set; }
		[SerializeField] private MerchantShopItemDataSo[] _items;
		
		public IReadOnlyList<MerchantShopItemDataSo> Items => _items;
	}
}