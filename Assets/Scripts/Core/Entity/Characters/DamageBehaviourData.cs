using System;
using System.Collections.Generic;
using Core.HealthSystem;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Entity.Characters
{
    [CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(DamageBehaviourData), fileName = nameof(DamageBehaviourData))]
    public class DamageBehaviourData : ScriptableObject
    {
        [TableList]
        public DamageBehaviour[] Behaviours;
        public DamageBehaviour Fallback;
        [NonSerialized] private bool _cached;
        private Dictionary<DamageType, DamageBehaviour> _behaviours;

        public DamageBehaviour GetBehaviour(DamageType type)
        {
            if (!_cached) Cache();
            return !_behaviours.ContainsKey(type) ? Fallback : _behaviours[type];
        }

        private void Cache()
        {
            _behaviours = new();
            foreach (var behaviour in Behaviours)
            {
                _behaviours.Add(behaviour.Type,behaviour);
            }

            _cached = true;
        }

#if UNITY_EDITOR
        [OnInspectorInit]
        private void ClearCache()
        {
            _cached = false;
        }
#endif
    }
    
    [Serializable]
    public struct DamageBehaviour
    {
        [VerticalGroup("Main")] public DamageType Type;
        [VerticalGroup("Main")] public bool BlockInteraction;
        
        [VerticalGroup("VFX")]
        [TitleGroup("VFX/Sounds")]
        public bool UseSoundsArray;
        
        [VerticalGroup("VFX")]
        [TitleGroup("VFX/Sounds")]
        [HideIf("UseSoundsArray")]
        public AudioClip HitSound;
        
        [VerticalGroup("VFX")]
        [TitleGroup("VFX/Sounds")]
        [ShowIf("UseSoundsArray")]
        public AudioClip[] HitSounds;
        
        [VerticalGroup("VFX")]
        [TitleGroup("VFX/Particle")]
        [ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
        public string ParticleKey;

        [VerticalGroup("DMG Args")] public float DamageMultiply;
        [VerticalGroup("DMG Args")] [HorizontalGroup("DMG Args/Blood")] [Tooltip("Blood Loss Amount")] [LabelText("BlAmount")] public float BloodLossAmountMultiply;
        [VerticalGroup("DMG Args")] [HorizontalGroup("DMG Args/Blood")] [Tooltip("Blood Loss Time")] [LabelText("BlTime")] public float BloodLossTimeMultiply;
        [VerticalGroup("DMG Args")] public float KnockOutMultiply;
        [VerticalGroup("DMG Args")] public float HitForceMultiply;
        [VerticalGroup("DMG Args")] public float UnpinMultiply;
        [VerticalGroup("DMG Args")] public float DismemberMultiply;

        public AudioClip GetSound()
        {
            return UseSoundsArray ? HitSounds.GetRandom() : HitSound;
        }
        
        public bool TryGetSound(out AudioClip clip)
        {
            if (UseSoundsArray)
            {
                clip = HitSounds.GetRandom();
                return true;
            }

            clip = HitSound;
            return clip;
        }
        
        public void Apply(ref DamageArgs args)
        {
            args.Damage *= DamageMultiply;
            args.BloodLossAmount *= BloodLossAmountMultiply;
            args.BloodLossTime *= BloodLossTimeMultiply;
            args.KnockOut *= KnockOutMultiply;
            args.HitForce *= HitForceMultiply;
            args.Unpin *= UnpinMultiply;
            args.DismemberDamage *= DismemberMultiply;
        }
    }
    
    public enum DamageType
    {
        Range,
        Melee,
        Explosion,
        Impact,
        ExplosionFromBody,
        Generic,
        HardMelee
    }
}