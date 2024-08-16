using Core.Entity.Characters.Adapters;
using NodeCanvas.Framework;

namespace Core.Entity.Ai.AiActions
{
	public abstract class BaseAdapterAction<T> : ActionTask<BaseLifeEntityAdapter<T>> 
		where T : LifeEntity
	{
		
	}
}