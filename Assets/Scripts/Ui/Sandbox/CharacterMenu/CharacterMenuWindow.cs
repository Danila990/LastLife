using Ui.Resource;
using Ui.Sandbox.EnemyUi;
using Ui.Sandbox.JetPack;
using Ui.Sandbox.Pointer;
using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.TicketUI;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.CharacterMenu
{
	public class CharacterMenuWindow : WindowBase, IMenuWindow, IPopUp
	{
		public override string Name => nameof(CharacterMenuWindow);

		public CharacterMenuWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<CharacterMenuController>();
			AddController<BossHealthUiController>();
			AddController<ResourceUiController>();
			
			AddController<PointerController>();
			AddController<JetPackController>();
		}
	}
	
	public class MarketCharacterMenuWindow : CharacterMenuWindow
	{
		public override string Name => nameof(CharacterMenuWindow);

		public MarketCharacterMenuWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<CharacterMenuController>();
			//AddController<TicketCountController>();
			//AddController<FuelTanksCountController>();
			AddController<JetPackController>();
		}
	}
}