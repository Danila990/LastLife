using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Abstraction;
using VContainerUi.Messages;
using VContainerUi.Services;

namespace LostConnection
{
	public class InternetConnectionChecker<TWindow> : IAsyncStartable, IDisposable
		where TWindow : Window
	{
		private readonly IUiMessagesPublisherService _msgPublisher;
		private readonly BoolReactiveProperty _hasInternet;

		public InternetConnectionChecker(
			IUiMessagesPublisherService msgPublisher
		)
		{
			_msgPublisher = msgPublisher;

			_hasInternet = new BoolReactiveProperty(true);
			InternetConnection.HasInternetObservable = _hasInternet;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			await UniTask.Delay(10f.ToSec(), cancellationToken: cancellation);
			while (!cancellation.IsCancellationRequested)
			{
				var hasConnection = await InternetConnection.CheckInternetConnection(cancellation);
				if (!hasConnection)
				{
					_msgPublisher.OpenWindowPublisher.OpenWindow<TWindow>(UiScope.Project);
				}
				await UniTask.Delay(60f.ToSec(), cancellationToken: cancellation);
			}
		}
		
		public void Dispose()
		{
			_hasInternet?.Dispose();
			InternetConnection.HasInternetObservable = null;
		}
	}
}