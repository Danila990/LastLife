using System;
using Analytic;
using Banner;
using GameStateMachine.States.Impl.Project;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainerUi.Abstraction;
using VContainerUi.Messages;

namespace AgeConversation
{
	
	public class AgeUiView : UiView
	{
		public TextMeshProUGUI AgeTXT;
		public Button LeftPad;
		public Button RightPad;
		public Button SubmitButton;
		public Button CloseButton;
		public int MinAge;
		public int MaxAge;
	}
	
	public class AgeUiController : UiController<AgeUiView>, IBannerController, IDisposable
	{
		private readonly IAnalyticService _analyticService;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly IPublisher<MessageBackWindow> _backWindow;
		private readonly IPublisher<MessageOpenWindow> _openWindow;
		private readonly IntReactiveProperty _ageValue = new IntReactiveProperty();
		private CompositeDisposable _compositeDisposable;
		public const string AGE_CONVERSATION_SHOWN = "AGE_CONVERSATION_SHOWN";
		
		private ReactiveCommand _hideCommand;
		public IReactiveCommand<Unit> Hide => _hideCommand;
		
		public AgeUiController(
			IAnalyticService analyticService, 
			IPlayerPrefsManager playerPrefsManager,
			IPublisher<MessageBackWindow> backWindow,
			IPublisher<MessageOpenWindow> openWindow)
		{
			_analyticService = analyticService;
			_playerPrefsManager = playerPrefsManager;
			_backWindow = backWindow;
			_openWindow = openWindow;
		}
				
		public bool IsAvailable()
		{
			return _playerPrefsManager.GetValue(ProjectInitializeState.SESSION_COUNT, 0) > 1 &&
			       _playerPrefsManager.GetValue<bool>(AGE_CONVERSATION_SHOWN, false) == false;
		}
		
		public void ShowBanner()
		{
			_openWindow.OpenWindow<AgeWindow>();
		}

		public override void OnShow()
		{
			_compositeDisposable?.Dispose();
			_compositeDisposable = new CompositeDisposable();
			View.SubmitButton.interactable = false;
			_hideCommand = new ReactiveCommand().AddTo(_compositeDisposable);
			
			_ageValue.Subscribe(val =>
			{
				View.AgeTXT.text = val switch
				{
					0 => "__",
					< 10 => $"_{val.ToString()}",
					_ => val.ToString()
				};
			}).AddTo(_compositeDisposable);
			
			View.LeftPad.OnClickAsObservable().Subscribe(_ =>
			{
				_ageValue.Value--;
				_ageValue.Value = Mathf.Clamp(_ageValue.Value, View.MinAge, View.MaxAge);
			}).AddTo(_compositeDisposable);
			
			View.RightPad.OnClickAsObservable().Subscribe(_ =>
			{
				_ageValue.Value++;
				_ageValue.Value = Mathf.Clamp(_ageValue.Value, View.MinAge, View.MaxAge);
				View.SubmitButton.interactable = true;
			}).AddTo(_compositeDisposable);
			
			View.SubmitButton.OnClickAsObservable().Subscribe(Submit).AddTo(_compositeDisposable);
			View.CloseButton.OnClickAsObservable().Subscribe(Close).AddTo(_compositeDisposable);
			
			//_ageValue.Value = Mathf.Clamp(_ageValue.Value, View.MinAge, View.MaxAge);
		}
		
		private void Close(Unit obj)
		{
			_playerPrefsManager.SetValue(AGE_CONVERSATION_SHOWN, true);
			_hideCommand.Execute();
			_compositeDisposable?.Dispose();
			_backWindow.BackWindow();
		}
		
		private void Submit(Unit obj)
		{
			_analyticService.SendEvent($"Age:{_ageValue.Value}", _ageValue.Value);
			Close(default);
		}

		public void Dispose()
		{
			_ageValue?.Dispose();
			_compositeDisposable?.Dispose();
		}
	}
}