using System;
using System.Linq;
using Core.Map;
using Core.Quests.Tips.Impl;
using Core.Services.BossLoop;
using Installer;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Db.Map
{
	public class MapObject : MonoBehaviour
	{
		public MonoBehaviour[] Injectables;
		public BossRoomProvider BossRoomProvider;
		public CharacterSpawnPointProvider CharacterSpawnPoint;
		public GridBuilder GridBuilder;
		public MapBoundsObject[] MapBounds;
		public ZoneBoundsObject[] Zones;
		public QuestTipContext[] QuestTipContexts;
		
		public AbstractMapExterior[] MapExteriorGroups;
		
#if UNITY_EDITOR
		[Button]
		private void FindInjectables()
		{
			Injectables = gameObject.GetComponentsInChildren<MonoBehaviour>().Where(x => x is IInjectableTag).Select(x => x).ToArray();
		}
		
		[Button]
		private void FindMapBounds()
		{
			MapBounds = gameObject.GetComponentsInChildren<MapBoundsObject>();
		}
		
		[Button]
		private void FindZones()
		{
			Zones = gameObject.GetComponentsInChildren<ZoneBoundsObject>();
		}
#endif
	}

	[Serializable]
	public class CharacterSpawnPointProvider
	{
		public Transform MainSpanPoint;
		public Transform[] OtherSpawnPoints;
		public Transform TrainSpawnPoint;
	}
}