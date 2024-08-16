using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.SceneLoad
{
	public class SceneLoadWindow : WindowBase, IPopUp
	{
		public override string Name => nameof(SceneLoadWindow);

		public SceneLoadWindow(IObjectResolver container) : base(container)
		{
			
		}

		protected override void AddControllers()
		{
			AddController<SceneLoadController>();
		}
	}
}