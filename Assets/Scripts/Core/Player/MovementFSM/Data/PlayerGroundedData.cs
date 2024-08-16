using System;
using Core.Player.MovementFSM.Data.Stopping;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Player.MovementFSM.Data
{
	[Serializable]
	public class PlayerGroundedData
	{
		[field:SerializeField] [field:PropertyRange(0, 25f)] public float BaseSpeed { get; private set; } 
		[field: SerializeField] [field: Range(0f, 5f)] public float GroundToFallRayDistance { get; private set; } = 1f;
		[field: SerializeField] [field: Range(0f, 2f)] public float AimMoveSpeedMlp { get; private set; } = 0.7f;
		[field:SerializeField] public AnimationCurve SlopeSpeedAngles { get; private set; }
		[field:SerializeField] public PlayerRotationData RotationData { get; private set; }
		[field:SerializeField] public PlayerSprintData PlayerSprintData { get; private set; }
		[field:SerializeField] public PlayerWalkData WalkData { get; private set; }
		[field:SerializeField] public PlayerCarryingData CarryingData { get; private set; }
		[field:SerializeField] public PlayerRunData RunData { get; private set; }
		[field:SerializeField] public PlayerStopData StopData { get; private set; }
	}
}