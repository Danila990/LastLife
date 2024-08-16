using System;
using System.Collections.Generic;
using Core.Quests;
using Core.Quests.Tree;
using Core.Services;
using Db.Quests;
using Sirenix.OdinInspector;
using TMPro;
using Ui.Sandbox.Quests.Views.Single;
using Ui.Sandbox.SelectMenu;
using UniRx;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.Quests.Views.Menu
{
	public class QuestsMenuView : SelectMenuMainView
	{
		[BoxGroup("SceneRefs")]
		public Transform Container;
		[BoxGroup("SceneRefs")]
		public TextMeshProUGUI Title;
		[BoxGroup("Params")]
		public string CompletedText;

		[BoxGroup("Prefabs")]
		public IntQuestView IntQuestView;
		[BoxGroup("Prefabs")]
		public BoolQuestView BoolQuestView;
	}
	
	public class QuestsMenuController : SelectMenuController<QuestsMenuView>, IPostInitializable, IDisposable
	{
		private readonly IQuestService _questService;
		private readonly QuestFactory _factory;
		private readonly Dictionary<string, IQuestPresenter<QuestView>> _presenters;
		private QuestsProgressPresenter _progressPresenter;
		private IDisposable _disposable;
		private CompositeDisposable _presentersDisposable;
		
		public QuestsMenuController(
			IMenuPanelService menuPanelService,
			IQuestService questService
			)
			: base(menuPanelService)
		{
			_factory = new (this);
			_presenters = new();
			_questService = questService;
		}
		
		public void Dispose()
		{
			foreach (var presenter in _presenters.Values)
				presenter?.Dispose();
			_presenters.Clear();
			
			_presentersDisposable?.Dispose();
			_disposable?.Dispose();
		}
		
		public void PostInitialize()
		{
			_disposable = _questService.ActiveTree.Subscribe(OnActiveTreeChanged);
		}

		private void OnActiveTreeChanged(QuestsTree tree)
		{
			if (tree == null)
			{
				View.Title.text = View.CompletedText;
				return;
			}
			
			_progressPresenter?.Dispose();
			_progressPresenter = new QuestsProgressPresenter("{0}/{1}", tree.Data.Name, View.Title, _questService);
			Clear();
			CreateQuests(_questService.ActiveTree.Value.AllQuests.Values);
		} 

		private void CreateQuests(IEnumerable<QuestData> quests)
		{
			_presentersDisposable?.Dispose();
			_presentersDisposable = new CompositeDisposable();
			
			foreach (var questData in quests)
			{
				var questPresenter = _factory.CreateQuest(questData, _presentersDisposable);
				if(questPresenter != null)
					_presenters.Add(questData.ImplementationData.InlineId, questPresenter);
			}
		}

		private void Clear()
		{
			_presentersDisposable?.Dispose();
			
			foreach (var presenter in _presenters.Values)
				Object.Destroy(presenter.View.gameObject);

			_presenters.Clear();
		}
		
		private class QuestFactory
		{
			private readonly QuestsMenuController _controller;

			public QuestFactory(QuestsMenuController controller)
			{
				_controller = controller;
			}

			public IQuestPresenter<QuestView> CreateQuest(QuestData questData, CompositeDisposable disposable)
			{
				if (questData.ImplementationData.FinalNode.Value == 1)
					return CreateBoolQuest(questData, disposable);
				
				return CreateIntQuest(questData, disposable);
			}

			private IntQuestPresenter CreateIntQuest(QuestData questData, CompositeDisposable disposable)
			{
				var instance = Object.Instantiate(_controller.View.IntQuestView, _controller.View.Container);
				var finalNode = _controller._questService.ActiveTree.Value.GetFinalNode(questData.ImplementationData.InlineId);
				if (finalNode == null)
					return null;
				
				var presenter = new IntQuestPresenter(instance, questData, finalNode, disposable);
				return presenter;
			}

			private BoolQuestPresenter CreateBoolQuest(QuestData questData, CompositeDisposable disposable)
			{
				var instance = Object.Instantiate(_controller.View.BoolQuestView, _controller.View.Container);
				var finalNode = _controller._questService.ActiveTree.Value.GetFinalNode(questData.ImplementationData.InlineId);
				if (finalNode == null)
					return null;
				
				var presenter = new BoolQuestPresenter(instance, questData, finalNode, disposable);
				return presenter;
			}
		}

		private class QuestsProgressPresenter : IDisposable
		{
			private readonly string _format;
			private readonly string _treeName;
			private readonly TextMeshProUGUI _content;
			private readonly IQuestService _questService;
			private readonly CompositeDisposable _disposable;
			private int _maxProgress;
			private int _currentProgress;
		
			public QuestsProgressPresenter(string format, string treeName, TextMeshProUGUI content, IQuestService questService)
			{
				_disposable = new();
			
				_content = content;
				_questService = questService;
				_format = format;
				_treeName = treeName;
				TrackProgress();
				UpdateProgress();
			}

			public void Dispose()
			{
				_disposable?.Dispose();
			}
		
			private void TrackProgress()
			{
				_maxProgress = _questService.ActiveTree.Value.AllQuests.Count;
				_questService.ActiveTree.Value.CompletedQuests.ObserveAdd().Subscribe(OnQuestComplete).AddTo(_disposable);
			}

			private void OnQuestComplete(DictionaryAddEvent<string, QuestData> _)
			{
				UpdateProgress();
			}

			private void UpdateProgress()
			{
				ICollection<KeyValuePair<string,QuestData>> dict = _questService.ActiveTree.Value.CompletedQuests;
				_currentProgress = dict.Count;
				_content.text = string.Format($"{_treeName} {_format}", _currentProgress, _maxProgress);
			}
		}
	}

}
