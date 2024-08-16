using System;
using Core.Loot;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Head
{
	[Serializable]
	public class HeadData
	{
		public float MoveSpeed;
		public float Height;
		public AudioClip DeathSound;
		public AudioClip SpawnSound;
		
		public bool RandomizeDeathAnim;
		[ShowIf("RandomizeDeathAnim")]
		public AnimationClip[] DeathClips;

		public LootData Loot;
	}
}