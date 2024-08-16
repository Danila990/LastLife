using Ui.Resource;
using Ui.Sandbox.EnemyUi;
using Ui.Sandbox.JetPack;
using Ui.Sandbox.Pointer;
using Ui.Sandbox.SelectMenu;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.Quests.Views.Menu
{
	public class QuestsMenuWindow : WindowBase, IMenuWindow, IPopUp
	{
		public override string Name => nameof(QuestsMenuWindow);

		public QuestsMenuWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<QuestsMenuController>();
			AddController<BossHealthUiController>();
			AddController<ResourceUiController>();
			
			AddController<PointerController>();
			AddController<JetPackController>();
		}
	}
	
	public class MarketQuestsMenuWindow : QuestsMenuWindow
	{
		public override string Name => nameof(QuestsMenuWindow);

		public MarketQuestsMenuWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<QuestsMenuController>();
			AddController<ResourceUiController>();
			
			AddController<PointerController>();
			AddController<JetPackController>();
		}
	}
}
