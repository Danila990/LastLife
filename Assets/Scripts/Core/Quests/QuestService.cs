using System;
using System.Collections.Generic;
using System.Threading;
using Core.Quests.Tree;
using Core.Quests.Tree.Node;
using Db.Quests;
using UniRx;
using Utils;

namespace Core.Quests
{
	public interface IQuestService
	{
		public IReadOnlyDictionary<string, QuestsTree> Trees { get; }
		public IReactiveProperty<QuestsTree> ActiveTree { get; }
		public ReactiveCommand<QuestsTree> OnTreeComplete { get; }
		public ReactiveCommand<QuestData> OnQuestComplete { get; }
		public void SetActiveTree(QuestsTree tree);
		public (QuestData Data, ReactiveIntTreeNode Node) FindHighPriorityQuest();

	}
	
	public class QuestService : IQuestService, IDisposable
	{
		private readonly CancellationToken _token;
		private readonly Dictionary<string, QuestsTree> _trees;
		private readonly ReactiveProperty<QuestsTree> _activeTree;
		private readonly ReactiveCommand<QuestsTree> _onTreeComplete;
		private readonly ReactiveCommand<QuestData> _onQuestComplete;

		private CompositeDisposable _currentSub;
		
		public IReadOnlyDictionary<string, QuestsTree> Trees => _trees;
		public IReactiveProperty<QuestsTree> ActiveTree => _activeTree;
		public ReactiveCommand<QuestsTree> OnTreeComplete => _onTreeComplete;
		public ReactiveCommand<QuestData> OnQuestComplete => _onQuestComplete;

		public QuestService(InstallerCancellationToken installerCancellationToken)
		{
			_trees = new();
			_activeTree = new();
			_onTreeComplete = new();
			_onQuestComplete = new();
			
			_token = installerCancellationToken.Token;
		}

		public void SetActiveTree(QuestsTree tree)
		{
			_currentSub?.Dispose();
			_currentSub = new CompositeDisposable();
			tree.OnTreeComplete.SubscribeWithState(tree, OnTreeCompleteInternal).AddTo(_currentSub);
			tree.CompletedQuests.ObserveAdd().Subscribe(OnQuestCompleteInternal).AddTo(_currentSub);
			_activeTree.Value = tree;
			_trees.TryAdd(tree.Id, tree);
		}

		private void OnTreeCompleteInternal(Unit _, QuestsTree tree)
		{
			_onTreeComplete.Execute(tree);
		}
		
		private void OnQuestCompleteInternal(DictionaryAddEvent<string, QuestData> addEvent)
		{
			_onQuestComplete.Execute(addEvent.Value);
		}
		
		public (QuestData Data, ReactiveIntTreeNode Node) FindHighPriorityQuest()
		{
			if(ActiveTree.Value == null)
				return (null, null);
			
			var maxPriority = -1;
			var questId = string.Empty;
			foreach (var kpv in ActiveTree.Value.Priorities)
			{
				if (kpv.Value.ModifiedPriority <= maxPriority)
					continue;
				maxPriority = kpv.Value.ModifiedPriority;
				questId = kpv.Key;
			}
			var node = ActiveTree.Value.GetFinalNode(questId);
			var inlineId = node.GetInlineId();
			return ActiveTree.Value.TryGetActiveQuest(inlineId, out var data) ? (data, node) : (null, null);
		}
		
		public void Dispose()
		{
			_activeTree?.Dispose();
			_onQuestComplete?.Dispose();
			_onTreeComplete?.Dispose();
			_currentSub?.Dispose();
		}
	}
}
