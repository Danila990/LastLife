using Shop.ItemProvider;
using UnityEngine;
using Utils;

namespace Shop.Models
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(RemoveAdsScriptableItem), fileName = nameof(RemoveAdsScriptableItem))]
	public class RemoveAdsScriptableItem : ScriptableShopItem<RemoveAdsModel>
	{
		
	}
}