namespace Core.Boosts.Impl.Player
{
	public class PlayerValuedBoostImpl : Boost, IBoostImpl
	{
		public override BoostCategory Category => BoostCategory.Valued;
		public override string AdditionalName => "Player";


		public PlayerValuedBoostImpl(BoostArgs boostArgs) : base(boostArgs) { }

		protected override void ApplyEffectInternal() { }

		protected override void RemoveEffectInternal() { }
	}

}
