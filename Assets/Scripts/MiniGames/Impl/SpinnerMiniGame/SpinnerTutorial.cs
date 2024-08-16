using System;
using LitMotion;
using LitMotion.Extensions;
using MiniGames.Impl.SpinnerMiniGame.View;
using SharedUtils.PlayerPrefs;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace MiniGames.Impl.SpinnerMiniGame
{
	public class SpinnerTutorial : MonoBehaviour
	{
		private const string KEY = "SpinnerTutorial";
		[SerializeField] private RectTransform _tutorialHand;
		[SerializeField] private SpinnerMiniGameView _spinnerView;

		[Inject] private readonly IPlayerPrefsManager _playerPrefsManager;
		
		private IDisposable _disposable;
		private MotionHandle _handle;

		private void StopTutorial(bool suc)
		{
			_handle.IsActiveCancel();
			_disposable?.Dispose();
			if (suc)
			{
				_playerPrefsManager.SetValue(KEY, true);
			}
		}
		
		public void StartTutorial()
		{
			if (_playerPrefsManager.GetValue<bool>(KEY, false))
			{
				_tutorialHand.gameObject.SetActive(false);
				return;
			}
			
			_disposable?.Dispose();
			_disposable = _spinnerView.OnMiniGameEnd.Subscribe(StopTutorial);
			
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(_tutorialHand.localEulerAngles, new Vector3(0, 0, -90), 2f)
				.WithLoops(-1, LoopType.Incremental)
				.BindToLocalEulerAngles(_tutorialHand);
		}
	}
}