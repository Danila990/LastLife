using System;
using Core.Entity.Characters;
using Core.InputSystem;
using SharedUtils;
using UniRx;
using VContainer;

namespace Core.Boosts.Impl.Player
{
	public class PlayerHealBoostImpl : Boost, IBoostImpl
	{
		[Inject] private readonly IPlayerSpawnService _spawnService;

		private readonly TimeSpan _interval = 0.5f.ToSec(); 
		private IDisposable _disposable; 
		private CharacterContext Context => _spawnService.PlayerCharacterAdapter.CurrentContext;
		public override BoostCategory Category => BoostCategory.Special;
		public override string AdditionalName => "Player";
		
		public PlayerHealBoostImpl(BoostArgs boostArgs) : base(boostArgs) { }

		protected override void ApplyEffectInternal()
			=> StartHeal();

		protected override void RemoveEffectInternal()
			=> EndHeal();

		private void StartHeal()
		{
			_disposable = Observable
				.Interval(_interval)
				.Subscribe(_ => Heal());
		}

		private void EndHeal()
		{
			_disposable?.Dispose();
		}

		private void Heal()
		{
			var context = _spawnService.PlayerCharacterAdapter.CurrentContext;
			if (context == null)
			{
				_disposable?.Dispose();
				return;
			}
			
			context.Health.AddHealth(BoostArgs.Value);
		}
	}
}
