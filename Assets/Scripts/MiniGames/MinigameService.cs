using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.Characters;
using Core.InputSystem;
using MessagePipe;
using MiniGames.Interfaces;
using VContainer.Unity;

namespace MiniGames
{
	public interface IMiniGameService
	{
		void PlayMiniGame(string miniGameName, Action<bool> callback, CharacterContext context);
		bool TryGetMiniGamePercentage(out IObservable<float> miniGamePercentage);
	}
	
	public class MiniGameService : IMiniGameService, IDisposable, IInitializable
	{
		private readonly IReadOnlyList<IMiniGameProvider> _provider;
		private readonly ISubscriber<PlayerContextChangedMessage> _playerContextSub;
		private IMiniGameProvider _currentMiniGame;
		private IDisposable _disposable;

		public MiniGameService(IReadOnlyList<IMiniGameProvider> provider, ISubscriber<PlayerContextChangedMessage> playerContextSub)
		{
			_provider = provider;
			_playerContextSub = playerContextSub;
		}
		
		public void Initialize()
		{
			_disposable = _playerContextSub.Subscribe(OnPlayer);
		}
		
		private void OnPlayer(PlayerContextChangedMessage msg)
		{
			if (!msg.Created && _currentMiniGame is not null)
			{
				if (_currentMiniGame.IsPlaying)
				{
					_currentMiniGame.Interrupt();
				}
			}
		}

		public void PlayMiniGame(string miniGameName, Action<bool> callback, CharacterContext context)
		{
			_currentMiniGame = _provider.FirstOrDefault(p => p?.MiniGameName == miniGameName);
			if (_currentMiniGame is null)
				throw new ArgumentException($"Mini game {miniGameName} not found");
			
			_currentMiniGame.PlayMiniGame(callback, context);
		}
		
		public bool TryGetMiniGamePercentage(out IObservable<float> miniGamePercentage)
		{
			if (_currentMiniGame?.MiniGameWinPercentage is null)
			{
				miniGamePercentage = null;
				return false;
			}
			miniGamePercentage = _currentMiniGame.MiniGameWinPercentage;
			return true;
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}