using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using Utils;

namespace Banner
{
	public class BannerService : IBannerService
	{
		private readonly IEnumerable<IBannerController> _bannerControllers;
		
		public BannerService(IEnumerable<IBannerController> bannerControllers)
		{
			_bannerControllers = bannerControllers;
		}
		
		public async UniTask ShowBanners(CancellationToken token)
		{
			foreach (var bannerController in _bannerControllers)
			{
				if (!bannerController.IsAvailable())
				{
					continue;
				}

				bannerController.ShowBanner();
				var task = new UniTask<Unit>(new AsyncCustomMessageHandler<Unit>(bannerController.Hide, token), 0);
				await task;
				await UniTask.Delay(0.5f.ToSec(), cancellationToken: token);
			}
		}
	}
}