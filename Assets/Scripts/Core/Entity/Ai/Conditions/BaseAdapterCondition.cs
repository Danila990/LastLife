using Core.Entity.Characters.Adapters;
using NodeCanvas.Framework;

namespace Core.Entity.Ai.Conditions
{
	public abstract class BaseAdapterCondition<T> : ConditionTask<BaseLifeEntityAdapter<T>> 
		where T : LifeEntity
	{
		
	}
}