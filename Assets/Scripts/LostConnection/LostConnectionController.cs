using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using UnityEngine;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Abstraction;
using VContainerUi.Messages;
using VContainerUi.Services;
using DelayType = Cysharp.Threading.Tasks.DelayType;

namespace LostConnection
{
	public class LostConnectionController : UiController<LostConnectionView>, IStartable, IDisposable
	{
		private readonly IUiMessagesPublisherService _publisherService;
		private IDisposable _disposable;
		private MotionHandle _rotationHandle;

		public LostConnectionController(IUiMessagesPublisherService publisherService)
		{
			_publisherService = publisherService;
		}
		
		public void Start()
		{
			_disposable = View.Button.OnClickAsAsyncEnumerable().Subscribe(Action);
		}

		private async UniTaskVoid Action(AsyncUnit arg1, CancellationToken arg2)
		{
			View.Button.gameObject.SetActive(false);
			View.Waiting.gameObject.SetActive(true);
			PlaySpinAnimation();
			await UniTask.Delay(1.5f.ToSec(), DelayType.UnscaledDeltaTime, cancellationToken: arg2);
			var hasConnection = await InternetConnection.CheckInternetConnection(arg2);
			DisposeAnim();
			
			if (hasConnection)
			{
				_publisherService.BackWindowPublisher.BackWindow(UiScope.Project);
			}
			else
			{
				View.Button.gameObject.SetActive(true);
				View.Waiting.gameObject.SetActive(false);
			}
		}

		public override void OnShow()
		{
			View.Button.gameObject.SetActive(true);
			View.Waiting.gameObject.SetActive(false);
		}
		
		public override void OnHide()
		{
			DisposeAnim();
		}
		
		private void PlaySpinAnimation()
		{
			DisposeAnim();
			_rotationHandle = LMotion
				.Create(Vector3.zero, new Vector3(0f, 0f, 360), 1f)
				.WithLoops(-1, LoopType.Incremental)
				.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
				.BindToLocalEulerAngles(View.WaitingRect);
		}
		
		private void DisposeAnim()
		{
			if (_rotationHandle.IsActive())
				_rotationHandle.Cancel();
		}

		public void Dispose()
		{
			DisposeAnim();
			_disposable?.Dispose();
		}
	}
}