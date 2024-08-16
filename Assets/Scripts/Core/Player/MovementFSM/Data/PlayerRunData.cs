using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Player.MovementFSM.Data
{
	[Serializable]
	public class PlayerRunData
	{
		[field: SerializeField] [field: PropertyRange(1, 2f)] public float SpeedModifier { get; private set; } = 1f;
	}
}