using System;
using System.Collections.Generic;
using Core.Quests.Tips;
using Core.Quests.Tips.Impl;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db.Quests
{
	public interface ITipObjectsData
	{
		public IEnumerable<TipObjectData> TipsData { get; }
	}
	
	[CreateAssetMenu(menuName = SoNames.QUESTS + nameof(TipObjectsDataSo), fileName = nameof(TipObjectsDataSo))]
	public class TipObjectsDataSo : ScriptableObject, ITipObjectsData
	{
		[TableList] [SerializeField]
		private TipObjectData[] _tipsData;

		public IEnumerable<TipObjectData> TipsData => _tipsData;
	}
	
	[Serializable]
	public struct TipObjectData
	{
		public string Id;
		public int PrewarmCount;
		public QuestTipObject Prefab;
	}
}
