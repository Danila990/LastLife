using Db.ObjectData;
using Shop.Models;

namespace Shop.ItemProvider
{
	public abstract class ScriptableInvItem<T> : ScriptableShopItem<ShopObjectItemModel<T>> 
		where T : ObjectData
	{
		public string ItemId => Model.Item.ObjectData.Id;
	}
	
	public abstract class ConsumableScriptableInvItem<T> : ScriptableShopItem<ConsumableItemModel<T>> 
		where T : ObjectData
	{
	}
}