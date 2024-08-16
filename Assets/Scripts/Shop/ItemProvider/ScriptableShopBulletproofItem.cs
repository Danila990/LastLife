using Core.Equipment.Data;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(ScriptableShopBulletproofItem), fileName = nameof(ScriptableShopBulletproofItem))]
	public class ScriptableShopBulletproofItem : ScriptableEquipmentShopItem<BulletproofItemData>
	{
		
	}

}