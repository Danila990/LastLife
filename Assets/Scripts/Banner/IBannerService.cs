using System.Threading;
using Cysharp.Threading.Tasks;

namespace Banner
{
	public interface IBannerService
	{
		UniTask ShowBanners(CancellationToken token);
	}
}