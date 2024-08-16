using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Quests.Save;
using Core.Quests.Tree;
using Cysharp.Threading.Tasks;
using Db.Quests;
using SharedUtils;
using UniRx;
using UnityEngine.Rendering;
using Utils;
using VContainer.Unity;

namespace Core.Quests
{
	public class QuestActivationService : IStartable
	{
		private readonly IAllQuestsData _allQuestsData;
		private readonly IQuestSaveAdapter _questSaveAdapter;
		private readonly IQuestService _questService;
		private readonly CancellationToken _token;
		private IDisposable _disposable;

		public QuestActivationService(
			IQuestSaveAdapter questSaveAdapter,
			IQuestService questService,
			IAllQuestsData allQuestsData,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_questSaveAdapter = questSaveAdapter;
			_questService = questService;
			_allQuestsData = allQuestsData;
			_token = installerCancellationToken.Token;
		}
		
		public void Start()
		{
			TryRestoreQuests();
		}

		private void OnTreeComplete(Unit _)
		{
			ActivateNextListAsync().Forget();
		}
		
		private async UniTaskVoid ActivateNextListAsync()
		{
			await UniTask.Delay(3f.ToSec(), cancellationToken: _token);
			ActivateNextList();
		}
		
		private void ActivateNextList()
		{
			var treeId = _questService.ActiveTree.Value.Id; 
			ActivateTree(GetNextQuestList(treeId));
		}

		private QuestsDataSo GetNextQuestList(string activeTreeId = null)
		{
			if (activeTreeId == null)
			{
				var activeTree = _questService.ActiveTree.Value;
				activeTreeId = activeTree?.Id;
			}
			
			if (activeTreeId != null)
			{
				var curListIndex = _allQuestsData.AllQuests.IndexOf(x => x.TreeData.Id == activeTreeId);
				curListIndex++;
				if (curListIndex.InBounds(_allQuestsData.AllQuests))
					return _allQuestsData.AllQuests.ElementAt(curListIndex);
			}

			return null;
		}
		
		private QuestsDataSo GetActiveQuestList(string activeTreeId)
		{
			if (!string.IsNullOrEmpty(activeTreeId))
			{
				var curListIndex = _allQuestsData.AllQuests.IndexOf(x => x.TreeData.Id == activeTreeId);
				if (curListIndex.InBounds(_allQuestsData.AllQuests))
					return _allQuestsData.AllQuests.ElementAt(curListIndex);
			}

			return _allQuestsData.AllQuests.First();
		}

		private void TryRestoreQuests()
		{
			_questSaveAdapter.TryGetActiveTreeId(out var treeId);
			
			if (_questSaveAdapter.TryGetTreeQuests(treeId, out var quests))
			{
				if (quests.All(x => x.IsComplete))
				{
					ActivateTree(GetNextQuestList(treeId));
					return;
				}
				
			}
			
			ActivateTree(GetActiveQuestList(treeId));
		}
		
		private void ActivateTree(QuestsDataSo questsDataSo)
		{
			if(questsDataSo == null)
				return;
			
			if (_questService.Trees.ContainsKey(questsDataSo.TreeData.Id))
				return;
			var tree = new QuestsTree(questsDataSo, _token);
			CreateQuests(tree, questsDataSo.QuestData);
			_questService.SetActiveTree(tree);
		}

		private void CreateQuests(QuestsTree newTree, IEnumerable<QuestData> quests)
		{
			_disposable?.Dispose();
			_disposable = newTree.OnTreeComplete.Subscribe(OnTreeComplete);

			var impl = ListPool<QuestData>.Get();
			foreach (var quest in quests)
			{
				var inlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, quest.ImplementationData.MidNodes, quest.ImplementationData.FinalNode.Id);
				if (_questSaveAdapter.TryGetQuest(newTree.Id, inlineId, out var savedQuestData))
				{
					var newQuest = new QuestData(quest.MainData, quest.ImplementationData);
					newQuest.ImplementationData.FinalNode.InitialValue = savedQuestData.Progress;
					impl.Add(newQuest);
					continue;
				}
				
				impl.Add(quest);
			}
			
			newTree.CreateQuests(impl);
			ListPool<QuestData>.Release(impl);
		}
	}
}
