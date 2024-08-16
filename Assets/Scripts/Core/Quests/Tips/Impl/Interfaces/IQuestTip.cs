using System.Collections.Generic;
using UnityEngine;

namespace Core.Quests.Tips.Impl.Interfaces
{
	public interface IQuestTip
	{
		public ICollection<string> QuestInlineIds { get; }
		public Vector3 Offset { get; }
		public Vector3 Rotation { get; }
		public Transform Origin { get; }
	}
}
