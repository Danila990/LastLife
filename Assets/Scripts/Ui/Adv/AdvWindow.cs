using JetBrains.Annotations;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;

namespace Ui.Adv
{
	[UsedImplicitly]
	public class AdvWindow : WindowBase, IPopUp
	{
		public override string Name => nameof(AdvWindow);

		public AdvWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<AdvController>();
		}
	}
}