using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Adv.Messages;
using Cysharp.Threading.Tasks;
using LitMotion;
using MessagePipe;
using SharedUtils;
using TMPro;
using UnityEngine;
using Utils;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Abstraction;
using VContainerUi.Messages;
using DelayType = Cysharp.Threading.Tasks.DelayType;
using Random = UnityEngine.Random;

namespace Ui.Adv
{
	public class AdvView : UiView
	{
		public TextMeshProUGUI Text;
		public string[] Words;
	}

	public class AdvController : UiController<AdvView>, IStartable, IDisposable
	{
		private readonly ISubscriber<MessageShowAdWindow> _subscriber;
		private readonly IPublisher<MessageOpenWindow> _publisher;
		private readonly IPublisher<MessageBackWindow> _backWindow;
		private readonly ISubscriber<MessageHideAd> _subscriberHideAd;
		private readonly Queue<string> _queueWords = new Queue<string>();
		private string _text;
		private MotionHandle _motion;

		public AdvController(
			ISubscriber<MessageShowAdWindow> subscriber,
			IPublisher<MessageOpenWindow> publisher,
			IPublisher<MessageBackWindow> backWindow,
			ISubscriber<MessageHideAd> subscriberHideAd)
		{
			_subscriber = subscriber;
			_publisher = publisher;
			_backWindow = backWindow;
			_subscriberHideAd = subscriberHideAd;
		}
		
		public void Start()
		{
			_subscriber.Subscribe(OnShowAdWindow).AddTo(View.destroyCancellationToken);
			_subscriberHideAd.Subscribe(OnHideAd).AddTo(View.destroyCancellationToken);
		}
		
		private void OnHideAd(MessageHideAd obj)
		{
			if (IsActive || InFocus)
			{
				_backWindow.BackWindow(UiScope.Project);
			}
		}

		private void OnShowAdWindow(MessageShowAdWindow msg)
		{
			_publisher.OpenWindow<AdvWindow>(UiScope.Project);
			AsyncAction(View.destroyCancellationToken, msg).Forget();
		}
		
		private async UniTaskVoid AsyncAction(CancellationToken token, MessageShowAdWindow msgCallback)
		{
			_text = GetText();
			_motion = LMotion
				.Create(3f, 0.01f, 1.5f)
				.WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
				.Bind(UpdateTimer);
			
			await UniTask.Delay(1.5f.ToSec(), DelayType.UnscaledDeltaTime, cancellationToken: token);

			msgCallback.Callback?.Invoke(msgCallback.OnComplete, msgCallback.InterstitialMeta);

			await UniTask.Delay(3f.ToSec(), DelayType.UnscaledDeltaTime, cancellationToken: token);

			OnHideAd(default);
		}

		public override void OnHide()
		{
			_motion.IsActiveCancel();
		}

		private void UpdateTimer(float value)
		{
			var textTimer = Mathf.Ceil(value).ToString(CultureInfo.InvariantCulture);
			View.Text.text = $"{_text}\n{textTimer}";
		}

		private string GetText()
		{
			if (_queueWords.TryDequeue(out var result))
			{
				return result;
			}
			GenerateWords();
			return _queueWords.Dequeue();
		}

		private void GenerateWords()
		{
			Array.Sort(View.Words, (s, s1) => (int)Random.value * 2);
			foreach (var word in View.Words)
			{
				_queueWords.Enqueue(word);
			}
		}
		
		public void Dispose()
		{
			_motion.IsActiveCancel();
		}
	}
}