using UniRx;

namespace Banner
{
	public interface IBannerController
	{
		bool IsAvailable();
		void ShowBanner();
		IReactiveCommand<Unit> Hide { get; } 
	}
}