using Core.Boosts.EndHandlers;
using Core.Boosts.Impl;
using Core.Entity.Characters;
using VContainer;

namespace Core.Boosts.Builder
{
	public abstract class BoostBuilder
	{
		protected readonly IObjectResolver Resolver;
		protected readonly CharacterContext Context;
		protected readonly CompositeBoostHandler Handler;
		
		protected BoostBuilder(IObjectResolver resolver, CharacterContext context)
		{
			Resolver = resolver;
			Context = context;
			Handler = new CompositeBoostHandler();
		}
		
		internal BoostBuilder WithEndOnTimer()
		{
			var temporaryHandler = new TemporaryEndBoostHandler(Handler.BoostImpl);
			Resolver.Inject(temporaryHandler);
			Handler.AddEndHandler(temporaryHandler);

			return this;
		}
		
		internal BoostBuilder WithEndOnContextDeath()
		{
			var deathHandler = new ContextDeathEndBoostHandler(Handler.BoostImpl, Context);
			Resolver.Inject(deathHandler);
			Handler.AddEndHandler(deathHandler);
			
			return this;
		}
		
		internal BoostBuilder WithEndOnContextDestroy()
		{
			var destroyHandler = new ContextDestroyEndBoostHandler(Handler.BoostImpl, Context);
			Resolver.Inject(destroyHandler);
			Handler.AddEndHandler(destroyHandler);

			return this;
		}
		
		public CompositeBoostHandler ToCompositeBoostHandler()
		{
			return Handler;
		}
	}
}
