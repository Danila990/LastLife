using Ui.Resource;
using VContainer;
using VContainerUi.Abstraction;

namespace Dialogue.Ui
{
	public class DialogueWindow : WindowBase
	{
		public override string Name => nameof(DialogueWindow);

		public DialogueWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<DialogueUiController>();
			AddController<ResourceUiController>();
		}
	}
}