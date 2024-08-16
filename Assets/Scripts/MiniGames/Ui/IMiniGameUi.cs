using System;
using UnityEngine;

namespace MiniGames.Ui
{
	public interface IMiniGameUi
	{
		string MiniGameName { get; }
		GameObject MiniGameUi { get; }
		void StartMiniGame();
		IObservable<bool> OnMiniGameEnd { get; }
	}
}