using Core.Equipment;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(ScriptableShopHatItem), fileName = nameof(ScriptableShopHatItem))]
	public class ScriptableShopHatItem : ScriptableEquipmentShopItem<HatItemData>
	{
		
	}
}
