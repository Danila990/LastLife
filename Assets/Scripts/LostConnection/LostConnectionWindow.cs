using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace LostConnection
{
	public class LostConnectionWindow : WindowBase, IPopUp
	{
		public override string Name => nameof(LostConnectionWindow);

		public LostConnectionWindow(IObjectResolver container) : base(container)
		{
		}
		
		protected override void AddControllers()
		{
			AddController<LostConnectionController>();
		}
	}
}