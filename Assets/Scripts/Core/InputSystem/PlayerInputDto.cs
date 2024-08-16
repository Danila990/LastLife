using System;
using UnityEngine;

namespace Core.InputSystem
{
    [Serializable]
    public class PlayerInputDto
    {
        public Vector2 Move;
        public Vector2 MoveRaw;
        public Vector2 Look;
        public bool Sprint;
        public bool AnalogMovement;
    }
}