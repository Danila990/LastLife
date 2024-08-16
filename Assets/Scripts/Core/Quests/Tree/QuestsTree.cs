using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Quests.Priority;
using Core.Quests.Tree.Node;
using Cysharp.Threading.Tasks;
using Db.Quests;
using UniRx;
using UnityEngine;

namespace Core.Quests.Tree
{
	public class QuestsTree : IDisposable
	{
		public const string ROOT_ID = "Root";
		
		public readonly string Id;
		public readonly TreeData Data;
		
		private readonly CancellationToken _token;
		private readonly TreeNode _rootNode;
		
		private readonly Dictionary<string, QuestData> _allQuests;
		private readonly ReactiveDictionary<string, QuestData> _activeQuests;
		private readonly ReactiveDictionary<string, QuestData> _completedQuests;
		private readonly Dictionary<string, ReactiveIntTreeNode> _finalNodes;
		private readonly ReactiveDictionary<string, ModifiedPriorityArgs> _priorities;
		private readonly ReactiveCommand _onTreeComplete;

		public IReadOnlyDictionary<string, QuestData> AllQuests => _allQuests;
		public IReactiveDictionary<string, QuestData> ActiveQuests => _activeQuests;
		public IReactiveDictionary<string, QuestData> CompletedQuests => _completedQuests;
		public IReactiveCommand<Unit> OnTreeComplete => _onTreeComplete;
		
		public IReactiveDictionary<string, ModifiedPriorityArgs> Priorities => _priorities;
		public IReadOnlyDictionary<string, ReactiveIntTreeNode> FinalNodes => _finalNodes;


		public TreeNode Root => _rootNode;

		public QuestsTree(QuestsDataSo questsData, CancellationToken token)
		{
			_token = token;
			Data = questsData.TreeData;
			_finalNodes = new();
			_activeQuests = new();
			_allQuests = new();
			_completedQuests = new();
			_priorities = new();
			_onTreeComplete = new();
			
			_rootNode = new TreeNode(ROOT_ID);
			Id = questsData.TreeData.Id;
		}

		public ReactiveIntTreeNode GetFinalNode(string inlineId)
		{
			return _finalNodes.TryGetValue(inlineId, out var node) ? node : null;
		}

		public void CreateQuests(ICollection<QuestData> questData)
		{
			foreach (var quest in questData)
			{
				var inlineId = TreeUtils.CreateInlineId(ROOT_ID, quest.ImplementationData.MidNodes, quest.ImplementationData.FinalNode.Id);
				_allQuests.Add(inlineId, quest);
			}

			foreach (var quest in questData)
				CreateQuest(quest);
		}

		private void CreateQuest(QuestData questData)
		{
			var endOfBranch = _rootNode.AddBranchOrNodes(questData.ImplementationData.MidNodes.Select(x => x.Id).ToArray());
			var finalNode = CreateFinalNode(questData.ImplementationData.FinalNode);
			endOfBranch.Add(finalNode);

			var inlineId = finalNode.GetInlineId();
			
			if (!_finalNodes.TryAdd(inlineId, finalNode))
				throw new ArgumentException($"The {inlineId} already exist");
			
			_activeQuests.Add(inlineId, questData);
			_priorities.Add(
				inlineId,
				new ModifiedPriorityArgs(inlineId, questData.MainData.Priority, questData.MainData.Priority));
		
			finalNode.Subscribe(OnNodeStateChanged).AddTo(_token);
		}
		
		private void OnNodeStateChanged(ReactiveIntNodeArgs args)
		{
			if (args.Value < args.Node.MaxValue)
				return;
				
			var inlineId = args.Node.GetInlineId();
			if(_completedQuests.ContainsKey(inlineId))
				return;
			
			var activeQuest = _activeQuests[inlineId];
			_priorities.Remove(inlineId);
			_activeQuests.Remove(inlineId);
			_completedQuests.Add(inlineId, activeQuest);

			if (_completedQuests.Count == _allQuests.Count)
				_onTreeComplete.Execute();
		}

		public void ChangePriority(ModifiedPriorityArgs args)
		{
			if(args.InitialPriority == 0 || args.ModifiedPriority == 0)
				return;

			if (_priorities.ContainsKey(args.InlineId))
				_priorities.Remove(args.InlineId);
			
			_priorities.Add(args.InlineId, args);
		}
		
		public bool TryGetActiveQuest(string inlineId, out QuestData data) => _activeQuests.TryGetValue(inlineId, out data);

		private static ReactiveIntTreeNode CreateFinalNode(FinalQuestNode nodeData)
			=> new ReactiveIntTreeNode(nodeData.Id, nodeData.Value, nodeData.InitialValue);

		public void Dispose()
		{
			_activeQuests?.Dispose();
			_completedQuests?.Dispose();
			_priorities?.Dispose();
			_onTreeComplete?.Dispose();
		}
	}
}
