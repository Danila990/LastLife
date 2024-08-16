using NodeCanvas.Framework;

namespace Core.Entity.Ai.Conditions
{
	public class LifeEntityIsAlive : ConditionTask<LifeEntity>
	{
		protected override bool OnCheck()
		{
			return agent && !agent.Health.IsDeath;
		}
	}
}