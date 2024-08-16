using System;
using Core.Entity.Characters;
using Cysharp.Threading.Tasks;
using MiniGames;
using UnityEngine;

namespace Tests.Mock
{
	public class MockMiniGameService : IMiniGameService
	{
		public void PlayMiniGame(string miniGameName, Action<bool> callback, CharacterContext context)
		{
			DelayComplete(callback).Forget();	
		}

		private async UniTaskVoid DelayComplete(Action<bool> callback)
		{
			await UniTask.DelayFrame(1);
			callback?.Invoke(true);
		}
		
		public bool TryGetMiniGamePercentage(out IObservable<float> miniGamePercentage)
		{
			miniGamePercentage = null;
			return false;
		}
	}
}