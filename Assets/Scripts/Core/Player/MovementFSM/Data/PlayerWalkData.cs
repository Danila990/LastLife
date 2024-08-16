using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Player.MovementFSM.Data
{
	[Serializable]
	public class PlayerWalkData
	{
		[field: SerializeField] [field: PropertyRange(0.15f, 1f)] public float SpeedModifier { get; private set; } = 0.5f;
	}

}