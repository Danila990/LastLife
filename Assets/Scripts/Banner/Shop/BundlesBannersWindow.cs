using VContainer;
using VContainerUi.Abstraction;

namespace Banner.Shop
{
	public class BundlesBannersWindow : WindowBase
	{
		public override string Name => nameof(BundlesBannersWindow);

		public BundlesBannersWindow(IObjectResolver container) : base(container)
		{
			
		}
		
		protected override void AddControllers()
		{
			AddController<ShopBundlesBannerUiController>();
		}
	}
}