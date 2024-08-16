using VContainer;
using VContainerUi.Abstraction;

namespace MiniGames.Ui
{
	public class MiniGameWindow : WindowBase
	{
		public override string Name => nameof(MiniGameWindow);

		public MiniGameWindow(IObjectResolver container) : base(container)
		{
		}
		
		protected override void AddControllers()
		{
			AddController<MiniGameUiController>();
		}
	}
}