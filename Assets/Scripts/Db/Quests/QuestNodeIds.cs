using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Db.Quests
{
	[CreateAssetMenu(menuName = SoNames.QUESTS + nameof(QuestNodeIds), fileName = nameof(QuestNodeIds))]
	public class QuestNodeIds : ScriptableObject
	{
		[SerializeField] private string[] _treeIds;
		[SerializeField] private string[] _midIds;

		public IEnumerable<string> TreeIds => _treeIds;
		public IEnumerable<string> MidIds => _midIds;
	}

	public static class FinalNodeIds
	{
		public const string FINAL_NODE_ID = "INT";
	}
	
}
