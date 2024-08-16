using System;
using System.Collections.Generic;
using Core.Etc;
using Db.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Services.BossLoop
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(BossLoopData), fileName = "BossLoopData")]
	public class BossLoopData : ScriptableObject, IBossLoopData
	{
		[SerializeField] private BossSpawnData[] _spawnData;
		public IReadOnlyList<BossSpawnData> SpawnData => _spawnData;
	}

	[Serializable]
	public class BossRoomProvider
	{
		[TabGroup("FX")] public AudioClip DoorOpening;
		[TabGroup("SceneRefs")] public Transform BossSpawnPoint;
		[TabGroup("SceneRefs")] public Transform BossLeftDoor;
		[TabGroup("SceneRefs")] public Transform BossRightDoor;
		[TabGroup("SceneRefs")] public float LOpenOffsetX;
		[TabGroup("SceneRefs")] public float ROpenOffsetX;
		[TabGroup("SceneRefs")] public float LCloseOffsetX;
		[TabGroup("SceneRefs")] public float RCloseOffsetX;
		[TabGroup("SceneRefs")] public DeathTrigger DeathBounds;
	}
	
	[Serializable]
	public class BossSpawnData
	{
		public string BossId;
		public float BossDelaySpawn;
		public bool IsNew;
		
		[TitleGroup("Quest")] [HorizontalGroup("Quest/QuestParams")] [VerticalGroup("Quest/QuestParams/A")]
		public bool DependedOnQuest;
		[TitleGroup("Quest")] [HorizontalGroup("Quest/QuestParams")] [VerticalGroup("Quest/QuestParams/B")][HideLabel][ShowIf("DependedOnQuest")] [ValueDropdown("@QuestsUtils.GetAllQuests()")]
		public string TreeId;
	}
	
	public interface IBossLoopData
	{
		IReadOnlyList<BossSpawnData> SpawnData { get; }
	}
}