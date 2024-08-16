using Core.Equipment.Data;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(ScriptableShopJetPackItem), fileName = nameof(ScriptableShopJetPackItem))]
	public class ScriptableShopJetPackItem : ScriptableEquipmentShopItem<JetPackItemData>
	{
		
	}

}
