using System;
using UnityEngine;

namespace Core.Player.MovementFSM.Data
{
    [Serializable]
    public class PlayerSprintData
    {
        [field: SerializeField] [field: Range(1f, 3f)] public float SpeedModifier { get; private set; } = 1.7f;
        [field: SerializeField] [field: Range(0f, 2f)] public float RunToSprintTime { get; private set; } = 0.5f;
    }
}