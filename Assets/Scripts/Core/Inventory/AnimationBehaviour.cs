using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory
{
    [Serializable]
    public class AnimationBehaviour
    {
        public string Key;
        
        [HideIf("AnimatorType", AnimatorSelect.HandsAnimator)] 
        public bool EnableIK;
        public bool CanCancel;
        
        [HideIf("CanCancel")] 
        public float ExitTime;
        
        public AnimatorSelect AnimatorType;
        public AnimationClip Clip;
        public bool ReturnToIdle;
        
        [NonSerialized] public float CurrentExitTime;
        [NonSerialized] public int HashedKey;
        public bool BlockCrossFade;

        public void HashKey()
        {
            HashedKey = Animator.StringToHash(Key);
        }
    }
}