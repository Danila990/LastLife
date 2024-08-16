using Db.ObjectData;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(CharacterShopItem), fileName = nameof(CharacterShopItem))]
	public class CharacterShopItem : ScriptableInvItem<CharacterObjectData>
	{
		
	}
}