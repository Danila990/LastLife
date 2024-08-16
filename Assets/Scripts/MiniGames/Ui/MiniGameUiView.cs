using System;
using System.Collections.Generic;
using MessagePipe;
using UniRx;
using UnityEngine;
using VContainerUi.Abstraction;
using VContainerUi.Messages;

namespace MiniGames.Ui
{
	public class MiniGameUiView : UiView
	{
		public RectTransform ContentTarget;
		public RectTransform InactiveMiniGames;
	}

	public class MiniGameUiController : UiController<MiniGameUiView>
	{
		private readonly IPublisher<MessageOpenWindow> _openWindowPublisher;
		private readonly IPublisher<MessageBackWindow> _backWindow;
		private readonly Dictionary<string, IMiniGameUi> _miniGames = new Dictionary<string, IMiniGameUi>();
		
		private IMiniGameUi _currentMiniGame;
		private IDisposable _runtimeDis;
		public event Action<string, bool> OnMiniGameEnd;
		public MiniGameUiView GetView() => View;
		public IMiniGameUi GetByName(string name) => _miniGames[name];
		
		public MiniGameUiController(
			IPublisher<MessageOpenWindow> openWindowPublisher, 
			IPublisher<MessageBackWindow> backWindow)
		{
			_openWindowPublisher = openWindowPublisher;
			_backWindow = backWindow;
		}
		
		public bool TryPlayFromCash(string miniGameName)
		{
			if (_miniGames.TryGetValue(miniGameName, out var miniGameUi))
			{
				SetMiniGame(miniGameUi);
				return true;
			}
			return false;
		}

		public void SetMiniGame(IMiniGameUi miniGameUi)
		{
			_runtimeDis?.Dispose();
			
			_currentMiniGame = miniGameUi; 
			_miniGames[_currentMiniGame.MiniGameName] = _currentMiniGame;
			miniGameUi.MiniGameUi.SetActive(true);
			_openWindowPublisher.OpenWindow<MiniGameWindow>();

			if (miniGameUi.MiniGameUi.transform.parent != View.ContentTarget)
				miniGameUi.MiniGameUi.transform.SetParent(View.ContentTarget, false);
			
			miniGameUi.StartMiniGame();
			_runtimeDis = miniGameUi.OnMiniGameEnd.Subscribe(StopMiniGame);
		}

		public void StopMiniGame(bool isSuccess)
		{
			_runtimeDis?.Dispose();
			OnMiniGameEnd?.Invoke(_currentMiniGame.MiniGameName, isSuccess);
			
			_miniGames[_currentMiniGame.MiniGameName] = _currentMiniGame;
			_currentMiniGame.MiniGameUi.transform.SetParent(View.InactiveMiniGames, false);
			_currentMiniGame.MiniGameUi.SetActive(false);
			_currentMiniGame = null;
			_backWindow.BackWindow();
		}
	}
}