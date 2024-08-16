using Ui.Resource;
using Ui.Sandbox.Experience;
using Ui.Sandbox.InventoryUi;
using Ui.Sandbox.JetPack;
using Ui.Sandbox.PlayerInput;
using Ui.Sandbox.Pointer;
using Ui.Sandbox.Quests.Views.Widgets;
using Ui.Sandbox.ReloadUI;
using Ui.Sandbox.SelectMenuButtons;
using Ui.Sandbox.WorldSpaceUI;
using VContainer;

namespace Ui.Sandbox
{
	public class MarketHudWindow : SandboxMainWindow
	{
		public override string Name => nameof(SandboxMainWindow); 

		public MarketHudWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<WorldSpaceUiController>();
			AddController<QuestWidgetController>();
			AddController<SelectMenuButtonsController>();
			AddController<ExperienceUiController>();
			AddController<InventoryPreviewController>();
			AddController<JetPackController>();
			
			AddController<ResourceUiController>();

			AddController<PlayerInputController>();
			AddController<ReloadUIController>();
			
			
			AddController<PointerController>();
		}
	}
}