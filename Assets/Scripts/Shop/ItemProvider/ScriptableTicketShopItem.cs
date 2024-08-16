using Shop.Models;
using UnityEngine;
using Utils;

namespace Shop.ItemProvider
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(ScriptableTicketShopItem), fileName = nameof(ScriptableTicketShopItem))]
	public class ScriptableTicketShopItem : ScriptableShopItem<TicketModel>
	{
		
	}
}