using System;
using System.Threading;
using Core.HealthSystem;
using Core.InputSystem;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using Utils;
using VContainer.Unity;

namespace Core.PlayerDeath
{
	public class PlayerEventPublisher : IPostInitializable, IDisposable
	{
		private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;
		private readonly IPublisher<PlayerContextDeathMessage> _deathPublisher;
		private readonly IPublisher<PlayerContextDamageMessage> _damagePublisher;
		
		private readonly CancellationToken _token;

		private CompositeDisposable _disposable;
		
		public PlayerEventPublisher(
			ISubscriber<PlayerContextChangedMessage> subscriber,
			IPublisher<PlayerContextDeathMessage> deathPublisher,
			IPublisher<PlayerContextDamageMessage> damagePublisher,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_token = installerCancellationToken.Token;
			_subscriber = subscriber;
			_deathPublisher = deathPublisher;
			_damagePublisher = damagePublisher;
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
		
		public void PostInitialize()
		{
			_subscriber.Subscribe(OnContextChanged).AddTo(_token);
		}

		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			_disposable?.Dispose();
			
			if(!msg.Created)
				return;

			_disposable = new CompositeDisposable();
			msg.CharacterContext.Health.OnDeath.Subscribe(OnContextDeath).AddTo(_disposable);
			msg.CharacterContext.Health.OnDamage.Subscribe(OnContextDamage).AddTo(_disposable);
		}

		private void OnContextDeath(DiedArgs args)
		{
			_deathPublisher.Publish(new(args));
			_disposable?.Dispose();
		}
		
		private void OnContextDamage(DamageArgs args)
		{
			_damagePublisher.Publish(new(args));
		}
	}
}
