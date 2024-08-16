using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.Head;
using Core.Quests.Tree;
using Core.Quests.Tree.Node;
using Core.Services;
using Db.Quests;
using UniRx;

namespace Core.Quests.Priority
{
	public class BossPriorityModifier : QuestPriorityModifier
	{
		private readonly IBossSpawnService _bossSpawnService;
		private IDisposable _disposable;
		private HeadContext _currentBoss;

		public BossPriorityModifier(IBossSpawnService bossSpawnService, IQuestService questService)
			: base(questService)
		{
			_bossSpawnService = bossSpawnService;
		}

		public override void PostInitialize()
		{
			_disposable = _bossSpawnService.CurrentBoss.Subscribe(OnCurrentBossChanged);
		}

		public override void Dispose()
		{
			base.Dispose();
			_disposable?.Dispose();
		}

		private void OnCurrentBossChanged(HeadContext context)
		{
			if(context)
				OnBossSpawned(context);
			else
				OnBossDied();
		}

		private void OnBossSpawned(HeadContext context)
		{
			
			if (!GetNodes(context, out var tree, out var nodes))
				return;

			_currentBoss = context;
			foreach (var node in nodes)
			{
				if (node is not ReactiveIntTreeNode)
					 continue;

				var inlineId = node.GetInlineId();
				if (tree.TryGetActiveQuest(inlineId, out var data))
					IncreasePriority(_currentBoss.SourceId, data);
			}
		}

		private void OnBossDied()
		{
			if(_currentBoss == null)
				return;

			DecreasePriority(_currentBoss.SourceId);

			_currentBoss = null;
		}

		private bool GetNodes(HeadContext context, out QuestsTree tree, out IEnumerable<TreeNode> nodes)
		{
			tree = null;
			nodes = null;
			
			tree = QuestService.ActiveTree.Value;
			if (tree == null) return false;

			var args = tree.Root.FindLastNodeInSimilarBranchWithMissing("Kill", "Boss", context.SourceId);
			if (args.MissingIds.Count > 0) 
				return false;

			nodes = args.LastNode.BreadthFirstSearch();
			return true;
		}
		
		private void IncreasePriority(string bossId, in QuestData data)
		{
			if (ModifiedPriorities.ContainsKey(bossId))
				return;
			
			var priority = data.MainData.Priority * data.MainData.PriorityMult;
			var args = new ModifiedPriorityArgs(data.ImplementationData.InlineId, data.MainData.Priority, priority);
			Notify(args);
		}

		private void DecreasePriority(string bossId)
		{
			if (!ModifiedPriorities.TryGetValue(bossId, out var args))
				return;
			
			var modifiedArgs = new ModifiedPriorityArgs(args.InlineId, args.InitialPriority, args.InitialPriority);
			ModifiedPriorities.Remove(bossId);
			Notify(modifiedArgs);
		}
	}
}
