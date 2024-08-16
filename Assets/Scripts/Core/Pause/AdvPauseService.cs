using System;
using Adv.Messages;
using LostConnection;
using MessagePipe;
using UniRx;
using VContainer.Unity;

namespace Core.Pause
{
	public interface IAdvPauseService
	{
		
	}
	
	public class AdvPauseService : IAdvPauseService, IStartable, IDisposable
	{
		private readonly IPauseService _pauseService;
		private readonly ISubscriber<MessageShowAd> _showAdSubscriber;
		private readonly ISubscriber<MessageHideAd> _hideAdSubscriber;
		private readonly ISubscriber<MessageShowAdWindow> _showAdvWindow;
		private readonly CompositeDisposable _disposables = new CompositeDisposable();

		private PauseArgs _args;

		public AdvPauseService(
			IPauseService pauseService,
			ISubscriber<MessageShowAd> showAdSubscriber,
			ISubscriber<MessageHideAd> hideAdSubscriber,
			ISubscriber<MessageShowAdWindow> showAdvWindow)
		{
			_pauseService = pauseService;
			_showAdSubscriber = showAdSubscriber;
			_hideAdSubscriber = hideAdSubscriber;
			_showAdvWindow = showAdvWindow;
		}

		public void Start()
		{
			_showAdSubscriber.Subscribe(_ => OnAdvShow()).AddTo(_disposables);
			_hideAdSubscriber.Subscribe(_ => OnAdvHide()).AddTo(_disposables);
			_showAdvWindow.Subscribe(_ => OnAdvShow()).AddTo(_disposables);
			InternetConnection.HasInternetObservable?.Subscribe(ChangeHasInternet).AddTo(_disposables);
		}
		
		private void ChangeHasInternet(bool obj)
		{
			_args.IsBackground = !obj;
			_pauseService.SetPause(ref _args);
		}

		public void Dispose()
			=> _disposables?.Dispose();

		private void OnAdvShow()
		{
			_args.IsAdv = true;
			_pauseService.SetPause(ref _args);
		}

		private void OnAdvHide()
		{
			_args.IsAdv = false;
			_pauseService.SetPause(ref _args);
		}
	}
}
