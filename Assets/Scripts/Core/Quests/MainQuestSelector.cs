using System;
using System.Collections;
using System.Threading;
using Core.Quests.Tree;
using Cysharp.Threading.Tasks;
using Db.Quests;
using UniRx;
using Utils;
using VContainer.Unity;

namespace Core.Quests
{
	public interface IMainQuestSelector
	{
		public IReactiveProperty<QuestData> MainQuest { get; }
	}
	
	public class MainQuestSelector : IMainQuestSelector, IPostInitializable, IDisposable
	{
		private readonly ReactiveProperty<QuestData> _mainQuest;
		private readonly IQuestService _questService;
		private readonly CancellationToken _token;
		private CompositeDisposable _disposable;

		public IReactiveProperty<QuestData> MainQuest => _mainQuest;

		public MainQuestSelector(IQuestService questService, InstallerCancellationToken installerCancellationToken)
		{
			_mainQuest = new ();

			_token = installerCancellationToken.Token;
			_questService = questService;
		}

		public void PostInitialize()
		{
			_questService.ActiveTree.Subscribe(OnTreeChanged).AddTo(_token);
		}

		private void OnTreeChanged(QuestsTree tree)
		{
			if(tree == null)
				return;
			
			_disposable?.Dispose();
			_disposable = new CompositeDisposable();
			tree.Priorities.ObserveAdd().Subscribe(_ => SwitchMainQuest()).AddTo(_disposable);
			tree.CompletedQuests.ObserveAdd().Subscribe(_ => SwitchMainQuest()).AddTo(_disposable);
			SwitchMainQuest();
		}
		
		private void SwitchMainQuest()
		{
			var pair = _questService.FindHighPriorityQuest();
			if(pair.Data == null)
			{
				_mainQuest.Value = null;
				return;
			}

			if (_mainQuest.Value == null)
			{
				_mainQuest.Value = pair.Data;
				return;
			}

			if (_mainQuest.Value.ImplementationData.InlineId != pair.Data.ImplementationData.InlineId)
				_mainQuest.Value = pair.Data;
		}
		
		public void Dispose()
		{
			_mainQuest?.Dispose();
			_disposable?.Dispose();
		}
	}
}
