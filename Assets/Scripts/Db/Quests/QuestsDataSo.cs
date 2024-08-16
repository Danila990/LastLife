using System;
using System.Collections.Generic;
using System.Linq;
using Core.Quests.Tree;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db.Quests
{
	[CreateAssetMenu(menuName = SoNames.QUESTS + nameof(QuestsDataSo), fileName = nameof(QuestsDataSo))]
	public class QuestsDataSo : SerializedScriptableObject
	{
		[BoxGroup("SaveId")]
		[HideLabel]
		[SerializeField] 
		[GUIColor("@Color.yellow")]
		private TreeData _treeData;
		
		[TableList]
		[SerializeField]
		private QuestData[] _questData;
		
		public TreeData TreeData => _treeData;
		public IEnumerable<QuestData> QuestData => _questData;

		private void OnValidate()
		{
			foreach (var questData in _questData)
			{
				var ids = questData.ImplementationData.MidNodes.Select(x => x.Id);
				questData.ImplementationData.InlineId = TreeUtils.CreateInlineId("Root", ids, FinalNodeIds.FINAL_NODE_ID);
				questData.ImplementationData.FinalNode.Id = FinalNodeIds.FINAL_NODE_ID;
			}
		}
	}

	[Serializable]
	public struct QuestMainData
	{
		[FoldoutGroup("Description")] [HideLabel] [MultiLineProperty(2)]
		public string Description;

		[FoldoutGroup("Description")]
		public DisplayData DisplayData;
		[FoldoutGroup("Description")] [HideLabel] [PreviewField]
		public Sprite Icon;
		
		[FoldoutGroup("Priority")] [Min(1)]
		public int Priority;
		[FoldoutGroup("Priority")] [Min(1)]
		public int PriorityMult;
	}

	[Serializable]
	public struct DisplayData
	{
		public bool OverrideForList;
		[ShowIf("OverrideForList")]
		public float OverridenListWidth;
		public bool OverrideForWidget;
		[ShowIf("OverrideForWidget")]
		public float OverridenWidgetWidth;
		public Vector3 Rotation;

	}
	
	[Serializable]
	public struct QuestImplementationData
	{
		[HideLabel]
		public FinalQuestNode FinalNode;
		[ShowInInspector] [GUIColor("@GetAnalyticsIdColor()")]
		public string AnalyticId;
		[ReadOnly] [ShowInInspector] [GUIColor("@Color.cyan")]
		public string InlineId;
		[HideLabel] [DetailedInfoBox(FQT.M,FQT.D)]
		public QuestNode[] MidNodes;
		
		private Color GetAnalyticsIdColor()
		{
		#if UNITY_EDITOR
			return string.IsNullOrEmpty(AnalyticId) ? Color.red : Color.green;
		#else
			return Color.green;
		#endif
		} 
	}
	
	[Serializable]
	public class QuestData
	{
		public QuestMainData MainData;
		public QuestImplementationData ImplementationData;

		public QuestData(QuestMainData mainData, QuestImplementationData implementationData)
		{
			MainData = mainData;
			ImplementationData = implementationData;
		}
	}

	[Serializable]
	public struct QuestNode
	{
		[ValueDropdown("@QuestsUtils.GetMidNodeIds()")]
		[HideLabel]
		public string Id;
	}
	
	[Serializable]
	public struct TreeData
	{
		[HideLabel] [LabelText("Quest Tree Id")]
		public string Id;
		[HideLabel] [LabelText("Displayed name")]
		public string Name;
	}
	
	[Serializable]
	public struct FinalQuestNode
	{
		[ReadOnly]
		public string Id;
		public int Value;
		[HideInInspector]
		public int InitialValue;
	}
}
