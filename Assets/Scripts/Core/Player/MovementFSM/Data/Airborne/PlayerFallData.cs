using System;
using UnityEngine;

namespace Core.Player.MovementFSM.Data.Airborne
{
    [Serializable]
    public class PlayerFallData
    {
        [field: Tooltip("Having higher numbers might not read collisions with shallow colliders correctly.")]
        [field: SerializeField] [field: Range(0f, 30f)] public float FallSpeedLimit { get; private set; } = 10f;
        [field: SerializeField] [field: Range(0f, 100f)] public float MinimumDistanceToBeConsideredHardFall { get; private set; } = 3f;
        [field: SerializeField] [field: Range(0f, 100f)] public float HardFallingVelocityThreshold { get; private set; } = 20f;
        [field: SerializeField] [field: Range(0f, 100f)] public float DamageVelocityThreshold { get; private set; } = 10f;
    }
}