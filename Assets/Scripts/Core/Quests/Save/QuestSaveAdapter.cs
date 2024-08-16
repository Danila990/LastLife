using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Quests.Tree;
using Core.Services.SaveSystem;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;
using Utils;
using VContainer.Unity;

namespace Core.Quests.Save
{
	public interface IQuestSaveAdapter
	{
		public bool TryGetQuest(string treeId, string inlineId, out SavedQuestData questData);
		public bool TryGetActiveTreeId(out string treeId);
		public IReactiveProperty<bool> OnLoaded { get; }
		public bool TryGetTreeQuests(string treeId, out ICollection<SavedQuestData> questData);
		public bool IsCompleteTree(string treeId);
	}
	
	public class QuestSaveAdapter : IAutoLoadAdapter, IInitializable, IQuestSaveAdapter
	{
		private readonly IQuestService _questService;
		private readonly CancellationToken _token;
		private readonly QuestsTracker _questsTracker;
		private readonly BoolReactiveProperty _onLoaded;

		private DeserializedSavedData _deserializedSavedData;
		public bool CanSave => true;
		public string SaveKey => "Quests";
		
		public IReactiveProperty<bool> OnLoaded => _onLoaded;
		
		public QuestSaveAdapter(IQuestService questService, InstallerCancellationToken installerCancellationToken)
		{
			_questsTracker = new(questService);
			_onLoaded = new();
			
			_questService = questService;
			_token = installerCancellationToken.Token;
		}

		public void Initialize()
		{
			_questService.ActiveTree.Subscribe(OnActiveTreeChanged).AddTo(_token);
		}
		
		public string CreateSave()
		{
				var savedData = _questsTracker.GetData();
			try
			{
				return _questService == null ? string.Empty : JsonConvert.SerializeObject(savedData);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return _deserializedSavedData != null  ? JsonConvert.SerializeObject(_deserializedSavedData.PreviousData) : string.Empty;
			}
		}
		public void LoadSave(string value)
		{
			try
			{
				_deserializedSavedData = new DeserializedSavedData(JsonConvert.DeserializeObject<SavedData>(value));
				_questsTracker.SetPreviousData(_deserializedSavedData);
				_onLoaded.Value = true;
				return;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			
			_deserializedSavedData = null;
			_onLoaded.Value = true;

		}

		public bool TryGetQuest(string treeId, string inlineId, out SavedQuestData questData)
		{
			questData = default(SavedQuestData);
			
			if (_deserializedSavedData == null)
				return false;

			return _deserializedSavedData.TryGet(treeId, out var treeData) && treeData.TryGet(inlineId, out questData);
		}
		
		public bool TryGetTreeQuests(string treeId, out ICollection<SavedQuestData> questData)
		{
			questData = null;
			
			if (_deserializedSavedData == null)
				return false;

			_deserializedSavedData.TryGet(treeId, out var treeData);
			questData = treeData?.Quests.Values;
			return treeData != null;
		}
		
		public bool TryGetActiveTreeId(out string treeId)
		{
			treeId = string.Empty;
			
			if (_deserializedSavedData == null)
				return false;

			treeId = _deserializedSavedData.ActiveTreeId;

			return string.IsNullOrEmpty(treeId);
		}

		public bool IsCompleteTree(string treeId)
		{
			if (_deserializedSavedData == null)
				return false;

			return _deserializedSavedData.Trees.TryGetValue(treeId, out var tree) && tree.IsComplete;

		}
		
		private void OnActiveTreeChanged(QuestsTree tree)
		{
			if(tree == null)
				return;
			
			TrackTree(tree);
		}

		private void TrackTree(QuestsTree tree) 
			=> _questsTracker.AddTracker(tree);
		
		
		#region QuestTrackers

		private class QuestsTracker
		{
			private readonly IQuestService _questService;
			private readonly Dictionary<string, TreeTracker> _trackers;
			private DeserializedSavedData _previousData; 
			
			public QuestsTracker(IQuestService questService)
			{
				_trackers = new();
				_questService = questService;
			}

			public void SetPreviousData(DeserializedSavedData savedData)
			{
				_previousData = savedData;
			}
			
			public void AddTracker(QuestsTree tree)
			{
				if(_trackers.ContainsKey(tree.Id))
					return;
				
				_trackers.Add(tree.Id, new TreeTracker(tree));
			}

			public SavedData GetData()
			{
				if (_questService.ActiveTree?.Value == null)
					return _previousData.PreviousData;
				
				var currentData = _trackers.Values.Select(x => x.GetData()).ToDictionary(x => x.Id, x => x);

				if (_previousData != null)
				{
					RefreshData(currentData);
				}
				
				return new SavedData(_questService.ActiveTree.Value.Id, currentData.Values.ToArray());
			}
			private void RefreshData(Dictionary<string, QuestTreeData> currentData)
			{
				foreach (var previousTree in _previousData.Trees)
				{
					if (currentData.ContainsKey(previousTree.Key))
						continue;

					currentData.Add(previousTree.Key, previousTree.Value.SavedTree);
				}
			}
		}
		
		private class TreeTracker
		{
			private readonly QuestsTree _tree;

			public TreeTracker(QuestsTree tree)
			{
				_tree = tree;
			}

			public QuestTreeData GetData()
			{
				return _tree == null
					? default(QuestTreeData)
					: new QuestTreeData(_tree.Id, ParseData());
			}

			private SavedQuestData[] ParseData()
				=> _tree.FinalNodes.Select(pair
					=> new SavedQuestData(pair.Key, pair.Value.Value >= pair.Value.MaxValue, pair.Value.Value)).ToArray();
		}
		
		#endregion
		
		#region SavedData

		private class DeserializedSavedData
		{
			internal readonly Dictionary<string, DeserializedTreeData> Trees;
			internal readonly string ActiveTreeId;
			internal readonly SavedData PreviousData;
			
			public DeserializedSavedData(in SavedData savedData)
			{
				Trees = new();
				ActiveTreeId = savedData.ActiveTreeId;
				PreviousData = savedData;
				ParseData(savedData);
			}
			
			public bool TryGet(string treeId, out DeserializedTreeData questData)
				=> Trees.TryGetValue(treeId, out questData);
			
			private void ParseData(in SavedData savedData)
			{
				var trees = savedData.Trees;
				if(trees == null || trees.Length == 0)
					return;
				
				foreach (var tree in trees)
				{
					if(Trees.ContainsKey(tree.Id))
						continue;
					if(string.IsNullOrEmpty(tree.Id))
						continue;
					if(tree.Quests == null || tree.Quests.Length == 0)
						continue;
					
					Trees.Add(tree.Id, new DeserializedTreeData(tree));
				}
			}

			
		}
		private class DeserializedTreeData
		{
			internal readonly Dictionary<string, SavedQuestData> Quests;
			internal readonly QuestTreeData SavedTree;
			internal readonly string Id;
			internal bool IsComplete = true;
				
			public DeserializedTreeData(in QuestTreeData treeData)
			{
				Quests = new();
				Id = treeData.Id;
				SavedTree = treeData;
				
				ParseData(treeData);
			}

			public bool TryGet(string inlineId, out SavedQuestData questData)
				=> Quests.TryGetValue(inlineId, out questData);
				
			private void ParseData(in QuestTreeData treeData)
			{
				foreach (var quest in treeData.Quests)
				{
					if(Quests.ContainsKey(quest.InlineId))
						continue;
					if(string.IsNullOrEmpty(quest.InlineId))
						continue;
					
					Quests.Add(quest.InlineId, quest);
					IsComplete &= quest.IsComplete;
				}
			}
		}
		
		[Serializable]
		private struct SavedData
		{
			public string ActiveTreeId;
			public QuestTreeData[] Trees;

			public SavedData(string activeTreeId, QuestTreeData[] trees)
			{
				ActiveTreeId = activeTreeId;
				Trees = trees;
			}
		}
		
		[Serializable]
		private struct QuestTreeData
		{
			public string Id;
			public SavedQuestData[] Quests;

			public QuestTreeData(string id, SavedQuestData[] quests)
			{
				Id = id;
				Quests = quests;
			}
		}
		#endregion

	}
	
	[Serializable]
	public struct SavedQuestData
	{
		public string InlineId;
		public bool IsComplete;
		public int Progress;

		public SavedQuestData(string inlineId, bool isComplete, int progress)
		{
			InlineId = inlineId;
			IsComplete = isComplete;
			Progress = progress;
		}
	}
}
