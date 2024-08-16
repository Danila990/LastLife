using System;
using Core.Entity;
using Core.Entity.Characters;

namespace Core.HealthSystem
{
	[Serializable]
	public struct SerilizedEffectArgs
	{
		public EffectType EffectType;
		public float Duration;
		
		public EffectArgs GetArgs(EntityContext context)
		{
			return new EffectArgs(context, EffectType, Duration);
		}
	}
	
	public readonly struct EffectArgs
	{
		public readonly EntityContext DamageSource;
		public readonly EffectType EffectType;
		public readonly float Duration;

		public EffectArgs(EntityContext damageSource, 
			EffectType effectType = EffectType.None, float duration = 1f)
		{
			DamageSource = damageSource;
			EffectType = effectType;
			Duration = duration;
		}
	}
}
