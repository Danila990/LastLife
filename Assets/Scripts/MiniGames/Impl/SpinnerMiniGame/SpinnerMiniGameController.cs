using System;
using Core.Entity.Characters;
using Core.InputSystem;
using MiniGames.Impl.SpinnerMiniGame.View;
using MiniGames.Interfaces;
using MiniGames.Ui;
using Ui.Sandbox.PlayerInput;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace MiniGames.Impl.SpinnerMiniGame
{
	public class SpinnerMiniGameController : IMiniGameProvider, IDisposable
	{
		private readonly SpinnerMiniGameView _spinnerMiniGameViewPrefab;
		private readonly MiniGameUiController _uiController;
		private readonly PlayerInputController _playerInputController;
		private readonly FloatReactiveProperty _winPercentage = new FloatReactiveProperty();
		private readonly IPlayerInputProvider _playerInputProvider;
		private readonly IObjectResolver _resolver;
		private Action<bool> _cashedCallback;
		private CharacterContext _context;
		private IDisposable _disposable;
		
		public bool IsPlaying { get; private set; }
		public string MiniGameName => MiniGameNames.SPINNER;
		public IObservable<float> MiniGameWinPercentage => _winPercentage;

		public SpinnerMiniGameController(
			SpinnerMiniGameView spinnerMiniGameViewPrefab, 
			MiniGameUiController uiController,
			PlayerInputController playerInputController,
			IPlayerInputProvider playerInputProvider,
			IObjectResolver resolver)
		{
			_spinnerMiniGameViewPrefab = spinnerMiniGameViewPrefab;
			_uiController = uiController;
			_playerInputController = playerInputController;
			_playerInputProvider = playerInputProvider;
			_resolver = resolver;
		}
		
		public void Interrupt()
		{
			OnMiniGameEnd(MiniGameNames.SPINNER, false);
		}
		
		public void PlayMiniGame(Action<bool> callback, CharacterContext context)
		{
			IsPlaying = true;
			_context = context;
			_cashedCallback = callback;
			_uiController.OnMiniGameEnd += OnMiniGameEnd;
			context.Inventory.UnSelect();
			
			if (!_uiController.TryPlayFromCash(MiniGameNames.SPINNER))
				_uiController.SetMiniGame(_resolver.Instantiate(_spinnerMiniGameViewPrefab, _uiController.GetView().ContentTarget));
			
			_playerInputProvider.UseListener(null);
			_playerInputController.ActiveInputRig.Value.Deactivate();
			var miniGameUi = (SpinnerMiniGameView)_uiController.GetByName(MiniGameNames.SPINNER);
			_disposable = miniGameUi.MiniGameProgress.Subscribe(f => _winPercentage.Value = f);
		}

		private void OnMiniGameEnd(string miniGameName, bool status)
		{
			IsPlaying = false;
			_disposable?.Dispose();
			_uiController.OnMiniGameEnd -= OnMiniGameEnd;
			_playerInputController.ActiveInputRig.Value.Activate();
			_context.Inventory.TrySelectItem(0);
			_playerInputProvider.UseListener(_context.Adapter as IPlayerInputListener);

			if (MiniGameName != miniGameName)
				return;
			
			_cashedCallback?.Invoke(status);
			_winPercentage.Value = 0;
		}
		
		public void Dispose()
		{
			_winPercentage?.Dispose();
		}
	}
}