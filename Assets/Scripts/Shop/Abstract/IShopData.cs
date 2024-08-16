using System.Collections.Generic;
using Db.ObjectData;
using Shop.ItemProvider;
using Shop.Models;

namespace Shop.Abstract
{
	public interface IShopData
	{
		IReadOnlyList<ScriptableTicketShopItem> TicketShopItems { get; }
		IReadOnlyList<CharacterShopItem> CharacterShopItems { get; }
		IReadOnlyList<ScriptableShopBulletproofItem> ShopBulletproofItems { get; }
		IReadOnlyList<ScriptableShopJetPackItem> ShopJetpackItems { get; }
		IReadOnlyList<ScriptableShopBootsItem>  ShopBootsItems { get; }
		IReadOnlyList<ScriptableShopHatItem>  ShopHatItems { get; }
		IReadOnlyList<ScriptableBoostShopItem> BoostsItems { get; }
		IReadOnlyList<BundleScriptableShopItem> BundleScriptable { get; }
		RemoveAdsScriptableItem RemoveAdsModel { get; }
		IEnumerable<ShopItemModel> GetAllShopItemModels();
	}
	
	public interface IShopStorage
	{
		/// <summary>
		/// ShopItemsByCharacterId
		/// </summary>
		IReadOnlyDictionary<string, ShopObjectItemModel<CharacterObjectData>> CharacterShopItems { get; }
		IReadOnlyDictionary<string, IShopObjectItemModel> ObjectItemModels { get; }
	}
}