using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Db.Panel
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(PanelsData), fileName = "PanelsData")]
	public class PanelsData : ScriptableObject, IPanelsData
	{
		[SerializeField] private ItemsPanel[] _panels;
		[SerializeField] private TicketsShopPanel _ticketsShop;
		[SerializeField] private ShopPanel _shootersPanel;
		[SerializeField] private ShopPanel _equipmentPanel;
		[SerializeField] private ShopPanel _firstOpenPanel;	
		[SerializeField] private ShopPanel _boostsPanel;	
		[SerializeField] private ShopPanel _bundlePanel;	
		public IReadOnlyList<ItemsPanel> Panels => _panels;
		public TicketsShopPanel TicketsShopPanel => _ticketsShop;
		public ShopPanel ShootersPanel => _shootersPanel;
		public ShopPanel BoostsPanel => _boostsPanel;
		public ShopPanel EquipmentPanel => _equipmentPanel;
		public ShopPanel FirstOpenPanel => _firstOpenPanel;
		public ShopPanel BundlesPanel=> _bundlePanel;
	}

	public interface IPanelsData
	{
		IReadOnlyList<ItemsPanel> Panels { get; }
		TicketsShopPanel TicketsShopPanel { get; }
		ShopPanel ShootersPanel { get; }
		ShopPanel BoostsPanel { get; }
		ShopPanel EquipmentPanel { get; }
		ShopPanel FirstOpenPanel { get; }
		ShopPanel BundlesPanel { get; }
	}
}