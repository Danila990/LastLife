using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Db.Quests
{
	public interface IAllQuestsData
	{
		public ICollection<QuestsDataSo> AllQuests { get; }
	}
	
	[CreateAssetMenu(menuName = SoNames.QUESTS + nameof(AllQuestsSo), fileName = nameof(AllQuestsSo))]
	public class AllQuestsSo : ScriptableObject, IAllQuestsData
	{
		[SerializeField] private QuestsDataSo[] _allQuests;

		public ICollection<QuestsDataSo> AllQuests => _allQuests;
	}
}
