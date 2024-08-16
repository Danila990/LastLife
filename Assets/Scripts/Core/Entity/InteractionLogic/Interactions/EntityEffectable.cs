using Core.Entity.Characters;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class EntityEffectable : AbstractMonoInteraction
	{
		public EffectBehaviourData BehaviourData;
		public EntityContext TargetContext;

		public virtual void DoEffect(ref EffectArgs args)
		{
			//Debug.Log($"Do Effect on {TargetContext}");
			TargetContext.DoEffect(ref args);
		}
        
		public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			var res = visiter.Accept(this,ref meta);
			//Debug.Log($"Visit {visiter}");
			return res;
		}
	}
}
