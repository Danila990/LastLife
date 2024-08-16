using VContainer;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.LoadScreen
{
	public class LoadScreenWindow : WindowBase
	{
		public override string Name => "LoadScreenWindow";

		public LoadScreenWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<LoadScreenController>(); 
		}
	}
}