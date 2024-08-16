using System;
using Core.Boosts.Impl;
using Core.Boosts.Impl.Player;
using Core.Entity.Characters;
using VContainer;

namespace Core.Boosts.Builder
{
	public class PlayerBoostBuilder : BoostBuilder
	{
		public PlayerBoostBuilder(IObjectResolver resolver, CharacterContext context) : base(resolver, context)
		{
		}
		
		public PlayerBoostBuilder Create(in BoostArgs args)
		{
			switch (args.Type)
			{
				case BoostTypes.SPEED_UP:
					CreateValuedBoost(in args)
						.WithEndOnTimer()
						.WithEndOnContextDeath()
						.WithEndOnContextDestroy();
					break;
				
				case BoostTypes.JUMP_UP:
					CreateValuedBoost(in args)
						.WithEndOnTimer()
						.WithEndOnContextDeath()
						.WithEndOnContextDestroy();
					break;
				case BoostTypes.DAMAGE:
					CreateValuedBoost(in args)
						.WithEndOnTimer()
						.WithEndOnContextDeath()
						.WithEndOnContextDestroy();
					break;
				case BoostTypes.HP:
					CreateHPBoost(in args)
						.WithEndOnTimer()
						.WithEndOnContextDeath()
						.WithEndOnContextDestroy();
					break;
				default:
					throw new NotImplementedException($"Not Implemented {args.Type}");
			}

			return this;
		}

		private BoostBuilder CreateValuedBoost(in BoostArgs args)
		{
			var speedUpBoost = new PlayerValuedBoostImpl(args);
			Resolver.Inject(speedUpBoost);
			Handler.Boost = speedUpBoost;
			Handler.BoostImpl = speedUpBoost;
			return this;
		}
		
		private BoostBuilder CreateHPBoost(in BoostArgs args)
		{
			var speedUpBoost = new PlayerHealBoostImpl(args);
			Resolver.Inject(speedUpBoost);
			Handler.Boost = speedUpBoost;
			Handler.BoostImpl = speedUpBoost;
			return this;
		}
	}
}
