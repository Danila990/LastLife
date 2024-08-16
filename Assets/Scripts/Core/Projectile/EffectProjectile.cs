using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.HealthSystem;

namespace Core.Projectile
{
	public class EffectProjectile : SimpleProjectile
	{
		public SerilizedEffectArgs EffectArgs;

		public override InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta)
		{
			var args = EffectArgs.GetArgs(this);
			effectInteraction.DoEffect(ref args);
			return base.Accept(effectInteraction, ref meta);
		}
	}
}
