using System;
using System.Collections.Generic;
using System.Linq;
using Db.Quests;

namespace Core.Quests.Tree
{
	public static class QuestsUtils
	{
		private static IEnumerable<string> _startIds = Array.Empty<string>();
		private static IEnumerable<string> _midIds = Array.Empty<string>();
		private static IEnumerable<string> _questsLists = Array.Empty<string>();
		private static IEnumerable<string> _questsIdsLists = Array.Empty<string>();
		private static IEnumerable<string> _tipObjects = Array.Empty<string>();
		
		#if UNITY_EDITOR
		private const string QUEST_NODE_IDS_PATH = @"Assets/Settings/Data/Quests/Info/NodeIds.asset";
		private const string QUESTS_LISTS_IDS_PATH = @"Assets/Settings/Data/Quests/AllQuestsSo.asset";
		private const string TIP_OBJECTS_PATH = @"Assets/Settings/Data/Quests/Info/TipObjectsDataSo.asset"; 
		#endif
		
		public static IEnumerable<string> GetStartNodeIds()
		{
			#if UNITY_EDITOR
			_startIds = GetNodeIdsAsset().TreeIds;
			#endif
			return _startIds;
		}
		
		public static IEnumerable<string> GetMidNodeIds()
		{
			#if UNITY_EDITOR
			_midIds = GetNodeIdsAsset().MidIds;
			#endif
			return _midIds;
		}
		
		public static IEnumerable<string> GetAllQuests()
		{
			#if UNITY_EDITOR
			_questsLists = GetAllQuestsAsset().AllQuests.Select(x => x.TreeData.Id);
			#endif
			return _questsLists;
		}

		public static IEnumerable<string> GetAllQuestsInlineId()
		{
			#if UNITY_EDITOR
			_questsIdsLists = GetAllQuestsAsset().AllQuests
				.SelectMany(x => x.QuestData)
				.Select(x => x.ImplementationData.InlineId);
			#endif
			return _questsIdsLists;
		}
		
		public static IEnumerable<string> GetTipObjectsIds()
		{
			#if UNITY_EDITOR
			_tipObjects = GetTipObjectsAsset().TipsData
				.Select(x => x.Id);
			#endif
			return _tipObjects;
		}
		
		
		#if UNITY_EDITOR
		private static QuestNodeIds GetNodeIdsAsset() => UnityEditor.AssetDatabase.LoadAssetAtPath<QuestNodeIds>(QUEST_NODE_IDS_PATH);
		private static AllQuestsSo GetAllQuestsAsset() => UnityEditor.AssetDatabase.LoadAssetAtPath<AllQuestsSo>(QUESTS_LISTS_IDS_PATH);
		private static TipObjectsDataSo GetTipObjectsAsset() => UnityEditor.AssetDatabase.LoadAssetAtPath<TipObjectsDataSo>(TIP_OBJECTS_PATH);
		#endif
		
	}
}
