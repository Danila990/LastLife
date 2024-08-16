using System;
using Core.Boosts.Impl;
using Core.Entity.Characters;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Effects
{
	[CreateAssetMenu(menuName = SoNames.EFFECT_DATA + nameof(EffectsDataSo), fileName = nameof(EffectsDataSo))]
	public class EffectsDataSo : ScriptableObject
	{
		[TableList]
		[SerializeField, TitleGroup("Effect")] private EffectData[] _simpleEffects;
		[SerializeField, TitleGroup("Effect")] private EffectData _simpleFallback;
		
		[TableList]
		[SerializeField, TitleGroup("Boost")] private BoostEffectData[] _boostEffects;
		[SerializeField, TitleGroup("Boost")] private BoostEffectData _boostFallback;
		
		public EffectData GetEffect(EffectType type)
		{
			foreach (var effectData in _simpleEffects)
			{
				if(effectData.Type == type)
					return effectData;
			}
			
			return _simpleFallback;
		}
		
		public BoostEffectData GetBoostEffect(in BoostArgs boostArgs)
		{
			foreach (var effectData in _boostEffects)
			{
				if(effectData.BoostType == boostArgs.Type)
					return effectData;
			}
			
			return _boostFallback;
		}
	}

	[Serializable]
	public class EffectData
	{
		[ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
		public string ParticleKey;
		[BoxGroup("EffectType")] public EffectType Type;
		public string BoneToAttach;

        [BoxGroup("EffectType")] public bool IsEffectDamage;
        [BoxGroup("Effect"), ShowIf(nameof(IsEffectDamage))] public float Damage;
        [BoxGroup("Effect"), ShowIf(nameof(IsEffectDamage))] public int CountTick;
        [BoxGroup("Effect"), ShowIf(nameof(IsEffectDamage))] public float TickDelay;
        [BoxGroup("Effect"), ShowIf(nameof(IsEffectDamage))] public AudioClip SoundEffect;


        [BoxGroup("Sounds")] public bool UseSoundsCollection;
		[BoxGroup("Sounds"), Range(0f, 1f)] public float SpatialBlend = 1f;
		[BoxGroup("Sounds"), ShowIf(nameof(UseSoundsCollection))] public AudioClip[] SoundsCollection;
		[BoxGroup("Sounds"), HideIf(nameof(UseSoundsCollection))] public AudioClip SingleSound;
		public AudioClip Sound => UseSoundsCollection ? SoundsCollection.GetRandom() : SingleSound;
	}
	
	[Serializable]
	public class BoostEffectData : EffectData
	{
		[ValueDropdown("@BoostTypes.GetTypes()")]
		public string BoostType;
		[BoxGroup("Sounds")] public AudioClip[] ApplySound;
		
	}
}
