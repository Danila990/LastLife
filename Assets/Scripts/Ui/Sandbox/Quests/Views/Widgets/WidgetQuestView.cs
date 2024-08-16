using System;
using System.Threading;
using Core.Quests;
using Core.Quests.Tree;
using Core.Quests.Tree.Node;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using Db.Quests;
using LitMotion;
using SharedUtils;
using SharedUtils.PlayerPrefs.Impl;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.Quests.Views.Widgets
{

	public class WidgetQuestView : UiView
	{
		[BoxGroup("SceneRefs")]
		public Transform Container;
		[BoxGroup("SceneRefs")]
		public RectTransform CompleteEndPoint;
		
		[BoxGroup("Widgets")]
		[BoxGroup("Widgets/PreparingWidget")] [HideLabel] [LabelText("Offset")]
		public Vector2 PreparingWidgetOffsetY;
		[BoxGroup("Widgets/PreparingWidget")] [HideLabel] [LabelText("SpawnPosition")]
		public Vector2 PreparingWidgetSpawnPosition;
		[BoxGroup("Widgets/MainWidget")] [HideLabel] [LabelText("Offset")]
		public Vector2 MainWidgetOffsetY;
		[BoxGroup("Widgets/MainWidget")] [HideLabel] [LabelText("SpawnPosition")]
		public Vector2 MainWidgetSpawnPosition;
		[BoxGroup("Widgets/SecondWidget")] [HideLabel] [LabelText("Offset")]
		public Vector2 SecondWidgetOffsetY;
		[BoxGroup("Widgets/SecondWidget")] [HideLabel] [LabelText("SpawnPosition")]
		public Vector2 SecondWidgetSpawnPosition;

		[BoxGroup("Prefabs")]
		public QuestPopUpWidget PreparingWidget;
		[BoxGroup("Prefabs")]
		public QuestPopUpWidget QuestWidget;
	}

	public class QuestWidgetController : UiController<WidgetQuestView>, IDisposable, IStartable
	{
		private readonly ActiveQuestPresenter _questPresenter;
		private readonly IQuestService _questService;
		private readonly InMemoryPlayerPrefsManager _memoryPrefsManager;
		private readonly IInputService _inputService;
		private readonly CancellationToken _token;
		private QuestPopUpWidget _preparingWidget;
		private IDisposable _disposable;
		public ActiveQuestPresenter QuestPresenter => _questPresenter;

		private const string KEY = "preparing_widget_is_showed"; 

		public QuestWidgetController(
			IQuestService questService,
			InMemoryPlayerPrefsManager memoryPrefsManager,
			IInputService inputService,
			InstallerCancellationToken installerCancellationToken
			)
		{
			_questService = questService;
			_memoryPrefsManager = memoryPrefsManager;
			_inputService = inputService;
			_token = installerCancellationToken.Token;
			_questPresenter = new (_questService, this, _token);
		}
		
		public void Start()
		{
			_disposable = _inputService.ObserveGetAxis2D("Horizontal", "Vertical").Subscribe(OnPlayerMove);
		}

		private void OnPlayerMove(Vector2 input)
		{
			if(input == Vector2.zero)
				return;
			
			_disposable?.Dispose();
			ShowPreparingWidget(() =>
			{
				_questService.ActiveTree.Subscribe(OnActiveTreeChanged).AddTo(_token);
			});
		}
		
		private void OnActiveTreeChanged(QuestsTree tree)
		{
			if(tree == null)
				return;
			
			ShowActiveQuest();
		}

		private void ShowActiveQuest()
		{
			_questPresenter.Init();
		}
		
		private void ShowPreparingWidget(Action callback)
		{
			if (SceneUtil.IsSandboxSceneActive() && !_memoryPrefsManager.HasKey(KEY))
			{
				ShowPreparingWidgetAsync(callback).Forget();
				_memoryPrefsManager.SetValue(KEY, 1);
				return;
			}
			
			callback();
		}

		private async UniTaskVoid ShowPreparingWidgetAsync(Action callback)
		{
			_preparingWidget = UnityEngine.Object.Instantiate(View.PreparingWidget, View.Container);
			_preparingWidget.RectTransform.anchoredPosition = View.PreparingWidgetSpawnPosition;
			var delay = (_preparingWidget.MoveDuration + _preparingWidget.DelayBetweenTweens).ToSec();
			
			_preparingWidget.DoAlpha(View.PreparingWidgetOffsetY, 0, 1);
			_preparingWidget.DoScale(1.3f, 1);
			await UniTask.Delay(delay, cancellationToken: _token);
			
			_preparingWidget.DoAlpha(Vector2.zero, 1, 0);
			_preparingWidget.DoScale(1, 0.7f);
			await UniTask.Delay(_preparingWidget.MoveDuration.ToSec(), cancellationToken: _token);
			
			_preparingWidget.Dispose();
			UnityEngine.Object.Destroy(_preparingWidget.gameObject);
			callback();
		}
		
		public void Dispose()
		{
			_questPresenter?.Dispose();
			if(_preparingWidget) 
				_preparingWidget.Dispose();
		}
		
		public class ActiveQuestPresenter : IDisposable
		{
			private readonly IQuestService _questService;
			private readonly QuestWidgetController _widgetController;
			private readonly CancellationToken _token;

			private QuestPopUpWidget _mainQuestWidget;
			private QuestPopUpWidget _secondQuestWidget;

			private string _currentQuestId;
			private IDisposable _mainQuestObserving;
			private CompositeDisposable _disposable;

			private readonly ReactiveCommand _onMainQuestHided;
			private readonly ReactiveCommand<QuestData> _onMainQuestDisplayed;
			public IReactiveCommand<QuestData> OnMainQuestDisplayed => _onMainQuestDisplayed;
			public IReactiveCommand<Unit> OnMainQuestHided => _onMainQuestHided;

			public ActiveQuestPresenter(
				IQuestService questService,
				QuestWidgetController widgetController,
				CancellationToken token)
			{
				_onMainQuestDisplayed = new();
				_onMainQuestHided = new();
				
				_questService = questService;
				_widgetController = widgetController;
				_token = token;
			}

			public void Init()
			{
				DisposeWidgets();
				
				_currentQuestId = string.Empty;
				_disposable = new CompositeDisposable();
				_questService.ActiveTree.Value.Priorities.ObserveAdd().Subscribe(addEvent => SwitchMainQuestAsync(false).Forget()).AddTo(_disposable);
				_questService.ActiveTree.Value.CompletedQuests.ObserveAdd().Subscribe(addEvent => ShowSecondQuest(addEvent.Value)).AddTo(_disposable);
				ShowMainQuest().Forget();
			}

			private void OnQuestStateChanged(ReactiveIntNodeArgs args)
			{
				SwitchMainQuestAsync(args.Value >= args.Node.MaxValue).Forget();
			}

			private async UniTaskVoid SwitchMainQuestAsync(bool isComplete)
			{
				_onMainQuestHided.Execute();
				if (isComplete)
				{
					await SetCompleteWidget(_mainQuestWidget);
				}
				else
				{
					var questData = _questService.FindHighPriorityQuest().Data;
					if (questData.ImplementationData.InlineId == _currentQuestId)
						return;
					
					await HideWidget(_mainQuestWidget, Vector3.zero);
				}
				
				ShowMainQuest().Forget();
			}
			
			private async UniTaskVoid ShowMainQuest()
			{
				var quest = _questService.FindHighPriorityQuest();
				if(quest.Data == null || quest.Node == null)
					return;
				if(quest.Data.ImplementationData.InlineId == _currentQuestId)
					return;

				if(!_mainQuestWidget)
					CreateMainWidget();

				_currentQuestId = quest.Data.ImplementationData.InlineId;
				await ShowWidget(quest.Data, _mainQuestWidget, _widgetController.View.MainWidgetSpawnPosition, _widgetController.View.MainWidgetOffsetY);
				_onMainQuestDisplayed.Execute(quest.Data);
				_mainQuestObserving?.Dispose();
				_mainQuestObserving = quest.Node.Subscribe(OnQuestStateChanged);
			}

			private void ShowSecondQuest(QuestData quest)
			{
				if(quest.ImplementationData.InlineId == _currentQuestId)
					return;
				
				if(!_secondQuestWidget)
					CreateSecondWidget();
				else 
					_secondQuestWidget.gameObject.SetActive(true);
				
				ShowSecondQuestAsync(quest).Forget();
			}

			private async UniTaskVoid ShowSecondQuestAsync(QuestData quest)
			{
				await ShowWidget(quest, _secondQuestWidget, _widgetController.View.SecondWidgetSpawnPosition, _widgetController.View.SecondWidgetOffsetY);
				await SetCompleteWidget(_secondQuestWidget);
			}
			
			private async UniTask ShowWidget(QuestData quest, QuestPopUpWidget widget, Vector2 spawnPos, Vector2 offset)
			{
				if(!widget)
					return;
				
				widget.RectTransform.anchoredPosition = spawnPos;
				await widget.SetContent(in quest.MainData);
				widget.SetCheckImage(false);
				await widget.DoAlpha(offset, 0 ,1 ).ToUniTask(widget.destroyCancellationToken);
			}
			
			private async UniTask SetCompleteWidget(QuestPopUpWidget widget)
			{
				if(!widget)
					return;
				
				await widget.SetCheckImage(true);
				await UniTask.Delay(1f.ToSec(), cancellationToken: widget.destroyCancellationToken);
				await widget.RollUp().ToUniTask(_token);
				await UniTask.Delay(0.3f.ToSec(), cancellationToken: widget.destroyCancellationToken);
				var duration = widget.JumpToPoint(_widgetController.View.CompleteEndPoint.position);
				await UniTask.Delay(duration.ToSec(), cancellationToken: widget.destroyCancellationToken);
				widget.Dispose();
				UnityEngine.Object.Destroy(widget.gameObject);
				await UniTask.NextFrame(cancellationToken: _token);
			}
			
			private async UniTask HideWidget(QuestPopUpWidget widget, Vector2 offset)
			{
				if(!widget)
					return;
				
				await widget.DoAlpha(offset, 1, 0).ToUniTask(_token);
				widget.Dispose();
				UnityEngine.Object.Destroy(widget.gameObject);
				await UniTask.NextFrame(cancellationToken: _token);
			}
			
			private void CreateMainWidget()
				=> _mainQuestWidget = CreateWidget();
			
			private void CreateSecondWidget()
				=> _secondQuestWidget = CreateWidget();
			
			private QuestPopUpWidget CreateWidget()
				=> UnityEngine.Object.Instantiate(_widgetController.View.QuestWidget, _widgetController.View.Container);


			private void DisposeWidgets()
			{
				if(_mainQuestWidget) _mainQuestWidget.Dispose();
				if(_secondQuestWidget) _secondQuestWidget.Dispose();
				_mainQuestObserving?.Dispose();
				_disposable?.Dispose();
			}
			
			public void Dispose()
			{
				_onMainQuestDisplayed?.Dispose();
				_onMainQuestHided?.Dispose();

			}
		}


	}

	
}
