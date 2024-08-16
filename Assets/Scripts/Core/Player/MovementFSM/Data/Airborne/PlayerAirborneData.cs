using System;
using UnityEngine;

namespace Core.Player.MovementFSM.Data.Airborne
{
    [Serializable]
    public class PlayerAirborneData
    {
        [field: SerializeField] public PlayerJumpData JumpData { get; private set; }
        [field: SerializeField] public PlayerFallData FallData { get; private set; }
    }
}