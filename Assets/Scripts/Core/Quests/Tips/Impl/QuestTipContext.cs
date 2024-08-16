using System.Collections.Generic;
using Core.Quests.Tips.Impl.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Quests.Tips.Impl
{
	public class QuestTipContext : MonoBehaviour, IQuestTip
	{
		[ValueDropdown("@QuestsUtils.GetAllQuestsInlineId()")]
		[SerializeField] private string[] inlineId;
		
		public ICollection<string> QuestInlineIds => inlineId;
		[field: SerializeField] public Vector3 Offset { get; private set; }
		[field: SerializeField] public Vector3 Rotation { get; private set; }
		[field: SerializeField] public Transform Origin { get; private set;}
	}
}
