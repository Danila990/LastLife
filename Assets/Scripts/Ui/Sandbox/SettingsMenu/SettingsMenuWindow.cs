using Ui.Sandbox.SelectMenu;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.SettingsMenu
{
	public class SettingsMenuWindow : WindowBase, IMenuWindow, IPopUp
	{
		public override string Name => nameof(SettingsMenuWindow);

		public SettingsMenuWindow(IObjectResolver container) : base(container)
		{
		}
		
		protected override void AddControllers()
		{
			AddController<SettingsMenuUiController>();
		}
	}
}