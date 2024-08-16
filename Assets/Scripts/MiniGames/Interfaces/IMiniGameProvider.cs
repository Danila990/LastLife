using System;
using Core.Entity.Characters;
using JetBrains.Annotations;

namespace MiniGames.Interfaces
{
	public interface IMiniGameProvider
	{
		string MiniGameName { get; }
		void PlayMiniGame(Action<bool> callback, CharacterContext context);
		bool IsPlaying { get; }
		void Interrupt();
		[CanBeNull] IObservable<float> MiniGameWinPercentage { get; }
	}
}