using System;
using System.Collections.Generic;
using Core.HealthSystem;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Entity.Characters
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(EffectBehaviourData), fileName = nameof(EffectBehaviourData))]
	public class EffectBehaviourData : ScriptableObject
	{
		[TableList]
		public EffectBehaviour[] Behaviours;
		public EffectBehaviour Fallback;
		[NonSerialized] private bool _cached;
		private Dictionary<EffectType, EffectBehaviour> _behaviours;

		public EffectBehaviour GetBehaviour(EffectType type)
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
	public struct EffectBehaviour
	{
		[VerticalGroup("Main")] public EffectType Type;
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

		[VerticalGroup("Debuff Args")] public float EffectMultiply;

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
        
		public void Apply(ref EffectArgs args)
		{

		}
	}
	public enum EffectType
	{
		None,
		Electric,
		Fire
	}
}
