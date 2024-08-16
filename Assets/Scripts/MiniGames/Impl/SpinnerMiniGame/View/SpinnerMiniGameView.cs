using System;
using MiniGames.Ui;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Impl.SpinnerMiniGame.View
{
	public class SpinnerMiniGameView : MonoBehaviour, IMiniGameUi
	{
		[SerializeField] private Button _stopGameButton;
		[SerializeField] private float _degressToWin = 360 * 10f;
		[SerializeField] private SpinnerUi _spinnerUi;
		[SerializeField] private Image _progressBar;
		[SerializeField] private SpinnerTutorial _spinnerTutorial;
		
		private ReactiveCommand<bool> _onMiniGameEnd;
		private FloatReactiveProperty _miniGameProgress;
		public string MiniGameName => MiniGameNames.SPINNER;
		public GameObject MiniGameUi => gameObject;
		public IObservable<bool> OnMiniGameEnd => _onMiniGameEnd;
		public IObservable<float> MiniGameProgress => _miniGameProgress;
		private CompositeDisposable _compositeDisposable;

		public void StartMiniGame()
		{
			_compositeDisposable = new CompositeDisposable();
			
			_spinnerUi.ResetSpinner();
			_onMiniGameEnd = new ReactiveCommand<bool>().AddTo(_compositeDisposable);
			_miniGameProgress = new FloatReactiveProperty().AddTo(_compositeDisposable);
			_spinnerUi.Value.Subscribe(OnSpinnerValue).AddTo(_compositeDisposable);
			_stopGameButton.OnClickAsObservable().Subscribe(_ => OnClickStop()).AddTo(_compositeDisposable);
			
			_spinnerTutorial.StartTutorial();
		}
		
		private void OnSpinnerValue(int currentDegrees)
		{
			var degreesToWin = _degressToWin;
			
 #if UNITY_EDITOR
 			degreesToWin = 60f;
 #endif
			_progressBar.fillAmount = currentDegrees / degreesToWin;
			if (currentDegrees >= degreesToWin)
			{
				EndMiniGame(true);
			}
			
			_miniGameProgress.Value = _progressBar.fillAmount;
		}

		private void EndMiniGame(bool status)
		{
			_onMiniGameEnd?.Execute(status);
			_compositeDisposable?.Dispose();
		}

		private void OnClickStop()
		{
			EndMiniGame(false);
		}
	}
}