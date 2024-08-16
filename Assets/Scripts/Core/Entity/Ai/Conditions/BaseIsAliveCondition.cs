using Core.Entity.Characters.Adapters;
using NodeCanvas.Framework;

namespace Core.Entity.Ai.Conditions
{

	public abstract class BaseIsAliveCondition<T> : ConditionTask<BaseLifeEntityAdapter<T>> 
		where T : LifeEntity
	{
		protected override bool OnCheck()
		{
			return agent.CurrentContext && !agent.CurrentContext.Health.IsDeath;
		}
	}

}