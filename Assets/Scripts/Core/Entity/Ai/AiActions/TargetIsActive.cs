using NodeCanvas.Framework;

namespace Core.Entity.Ai.AiActions
{
	public class TargetIsActive : ConditionTask
	{
		public BBParameter<EntityContext> EntityTarget;

		protected override bool OnCheck()
		{
			var target = EntityTarget.value;
			if (target is LifeEntity lifeEntity)
			{
				return !lifeEntity.Health.IsDeath;
			}
			return target;
		}
	}

}