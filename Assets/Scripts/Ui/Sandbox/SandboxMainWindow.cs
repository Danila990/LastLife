using Ui.Resource;
using Ui.Sandbox.EnemyUi;
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
using VContainerUi.Abstraction;

namespace Ui.Sandbox
{
	public class SandboxMainWindow : WindowBase
	{
		public override string Name => nameof(SandboxMainWindow); 

		public SandboxMainWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<WorldSpaceUiController>();
			AddController<JetPackController>();
			AddController<BossHealthUiController>();
			AddController<QuestWidgetController>();
			AddController<SelectMenuButtonsController>();
			AddController<ExperienceUiController>();
			AddController<InventoryPreviewController>();
			AddController<ResourceUiController>();
			AddController<PlayerInputController>();
			AddController<ReloadUIController>();

			AddController<PointerController>();
		}
	}

}