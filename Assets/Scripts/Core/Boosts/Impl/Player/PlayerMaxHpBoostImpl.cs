using Core.Entity.Characters;
using Core.InputSystem;
using VContainer;

namespace Core.Boosts.Impl.Player
{
	public class PlayerMaxHpBoostImpl : Boost, IBoostImpl
	{
		[Inject] private readonly IPlayerSpawnService _spawnService;

		private float defaultMaxHealth;
		
		private CharacterContext Context => _spawnService.PlayerCharacterAdapter.CurrentContext;
		public override BoostCategory Category => BoostCategory.Special;
		public override string AdditionalName => "Player";
		
		public PlayerMaxHpBoostImpl(BoostArgs boostArgs) : base(boostArgs) { }

		protected override void ApplyEffectInternal()
		{
			defaultMaxHealth = Context.Health.MaxHealth;
			Context.Health.AddHealthWithoutClamp(BoostArgs.Value);
		}

		protected override void RemoveEffectInternal()
		{
			Context.Health.ClampHealth();
		}
	}

}
