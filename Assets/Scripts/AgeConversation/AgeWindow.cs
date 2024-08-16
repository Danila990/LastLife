using VContainer;
using VContainerUi.Abstraction;

namespace AgeConversation
{
	public class AgeWindow : WindowBase
	{
		public override string Name => nameof(AgeWindow);

		public AgeWindow(IObjectResolver container) : base(container) { }
		
		protected override void AddControllers()
		{
			AddController<AgeUiController>();
		}
	}
}