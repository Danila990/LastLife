using System;
using System.Collections.Generic;
using Core.Quests.Tips;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db.Quests
{
	public interface IQuestTipData
	{
		public ICollection<QuestTipData> TipsData { get; }
		public string FallbackId { get; }
	}
	
	[CreateAssetMenu(menuName = SoNames.QUESTS + nameof(QuestTipDataSo), fileName = nameof(QuestTipDataSo))]
	public class QuestTipDataSo : ScriptableObject, IQuestTipData
	{
		[ValueDropdown("@QuestsUtils.GetTipObjectsIds()")]
		public string _fallbackId;
		[TableList] [SerializeField]
		private QuestTipData[] _tipsData;

		public ICollection<QuestTipData> TipsData => _tipsData;
		public string FallbackId => _fallbackId;
	}

	[Serializable]
	public struct QuestTipData
	{
		[ValueDropdown("@QuestsUtils.GetAllQuestsInlineId()")]
		public string QuestInlineId;
		[ValueDropdown("@QuestsUtils.GetTipObjectsIds()")]
		public string TipObjectId;
	}

}
