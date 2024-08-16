using Db.ObjectData;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + "BOOST_ITEM", fileName = nameof(ScriptableBoostShopItem))]
	public class ScriptableBoostShopItem : ConsumableScriptableInvItem<BoostObjectData>
	{
		
	}
}