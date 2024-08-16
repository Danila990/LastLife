using Core.Equipment.Data;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(ScriptableShopBootsItem), fileName = nameof(ScriptableShopBootsItem))]
	public class ScriptableShopBootsItem : ScriptableEquipmentShopItem<BootsItemData>
	{
		
	}
}
