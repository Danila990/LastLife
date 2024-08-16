using Ui.Resource;
using Ui.Sandbox.EnemyUi;
using Ui.Sandbox.JetPack;
using Ui.Sandbox.Pointer;
using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.TicketUI;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.ShopMenu
{
	public class ShopMenuWindow : WindowBase, IMenuWindow, IPopUp
	{
		public override string Name => nameof(ShopMenuWindow);

		public ShopMenuWindow(IObjectResolver container) : base(container)
		{
		
		}
	
		protected override void AddControllers()
		{
			AddController<ShopMenuController>();
			AddController<BossHealthUiController>();
			AddController<ResourceUiController>();

			AddController<PointerController>();
			AddController<JetPackController>();
		}
	}
	
	public class MarketShopMenuWindow : ShopMenuWindow
	{
		public override string Name => nameof(ShopMenuWindow);

		public MarketShopMenuWindow(IObjectResolver container) : base(container)
		{
		
		}
	
		protected override void AddControllers()
		{
			AddController<ShopMenuController>();
			AddController<ResourceUiController>();

			AddController<JetPackController>();
		}
	}
}