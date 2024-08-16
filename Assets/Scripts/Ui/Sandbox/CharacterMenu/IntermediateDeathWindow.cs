using Ui.Sandbox.EnemyUi;
using VContainer;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.CharacterMenu
{
	public class IntermediateDeathWindow : WindowBase
	{
		public override string Name
		{
			get
			{
				return nameof(IntermediateDeathWindow);
			}
		}

		public IntermediateDeathWindow(IObjectResolver container) : base(container)
		{

		}

		protected override void AddControllers()
		{
			AddController<BossHealthUiController>();
		}
	}
	
	public class MarketIntermediateDeathWindow : IntermediateDeathWindow
	{
		public override string Name => nameof(IntermediateDeathWindow);

		public MarketIntermediateDeathWindow(IObjectResolver container) : base(container)
		{

		}

		protected override void AddControllers()
		{
		}
	}
}