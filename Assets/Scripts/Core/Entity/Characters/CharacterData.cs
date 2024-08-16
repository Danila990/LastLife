using Core.Entity.Step;
using UnityEngine;
using Utils;

namespace Core.Entity.Characters
{
    [CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(CharacterData), fileName = nameof(CharacterData))]
    public class CharacterData : ScriptableObject
    {
        public FootstepData FootstepData;
        public AudioClip DeathSound;
        public AudioClip HurtSound;
        public float GroundSphereOffset;
        public float GroundSphereRadius;
        public LayerMask GroundLayers;
        public Vector3 ColliderCenter;
        public float ColliderRadius;
        public float AgentRadius;
        public float ColliderHeight = 2;
        public float MoveSpeed;
        public float SprintSpeed;
        public float SpeedChangeRate;
        public float RotationSmoothTime;
    }
}