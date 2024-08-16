using UnityEngine;
using Utils;

namespace Db.Panel
{
	[CreateAssetMenu(menuName = SoNames.SHOP_PANEL + "ShopPanel", fileName = "ShopPanel")]
	public class ShopPanel : ScriptableObject, IShopPanelData
	{
		[field:SerializeField] public string PanelId { get; set; }
		[field:SerializeField] public Sprite PanelIcon { get; set; }
		[field:SerializeField] public string PanelName { get; set; }
	}
	
	public interface IShopPanelData
	{
		string PanelId { get; }
		Sprite PanelIcon { get; }
		string PanelName { get; }
	}
}