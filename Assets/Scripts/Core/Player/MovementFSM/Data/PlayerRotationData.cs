using System;
using UnityEngine;

namespace Core.Player.MovementFSM.Data
{
    [Serializable]
    public class PlayerRotationData
    {
        [field: SerializeField] public Vector3 TargetRotationReachTime { get; private set; }
    }
}