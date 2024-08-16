using Core.Entity;

namespace Core.Actions
{
	public abstract class GenericEntityAction<T> : AbstractScriptableEntityAction
		where T : EntityContext
	{
		protected new T CurrentContext { get; private set; }
		
		public override void SetContext(EntityContext context)
		{
			_settings.TryAdd(context.Uid, new InnerSettings());
			CurrentContext = (T)context;
		}
	}
}