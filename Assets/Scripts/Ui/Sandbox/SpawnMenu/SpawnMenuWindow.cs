using Ui.Resource;
using Ui.Sandbox.EnemyUi;
using Ui.Sandbox.JetPack;
using Ui.Sandbox.Pointer;
using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.TicketUI;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.SpawnMenu
{
	public class SpawnMenuWindow : WindowBase, IMenuWindow, IPopUp
	{
		public override string Name => nameof(SpawnMenuWindow);

		public SpawnMenuWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<SpawnMenuController>();
			AddController<BossHealthUiController>();
			AddController<ResourceUiController>();

			AddController<PointerController>();
			AddController<JetPackController>();
		}
	}
	
	public class MarketSpawnMenuWindow : SpawnMenuWindow
	{
		public override string Name => nameof(SpawnMenuWindow);

		public MarketSpawnMenuWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<SpawnMenuController>();
			AddController<ResourceUiController>();

			AddController<JetPackController>();
		}
	}
}