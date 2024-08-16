using Core.HealthSystem;

namespace Core.Entity.Characters
{
	public readonly struct MessageDamageEvent
	{
		public readonly DamageArgs DamageArgs;
		public readonly EntityContext AffectedEntity;
		
		public MessageDamageEvent(DamageArgs damageArgs, EntityContext affectedEntity)
		{
			DamageArgs = damageArgs;
			AffectedEntity = affectedEntity;
		}
	}
}