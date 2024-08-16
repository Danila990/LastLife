using Core.AnimationRigging;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Core.CameraSystem
{
    public class FirstPersonHands : MonoBehaviour
    {
        public MonoRigProvider RigProvider;
        public SkinnedMeshRenderer MeshRenderer;
        public SkinnedMeshRenderer LegMeshRenderer;
        public Animator HandAnimator;
        public Animator LegAnimator;
        public Transform RightHand;
        public Transform LeftHand;
        public RigBuilder RigBuilder;
    }
}