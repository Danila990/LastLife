using System;
using Core.Services;
using GameStateMachine.States.Impl.Project;
using Ui.Sandbox.Conversation;
using Ui.Widget;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SettingsMenu
{
	public class RestartSettingsPresenter : IDisposable
	{
		private readonly IDisposable _disposable;
		[Inject] private readonly ISceneLoaderService _sceneLoaderService;
		[Inject] private readonly IConversationController _conversationController;
		[Inject] private readonly ProjectStateMachine _projectState;
		public readonly Button RestartButton;

		public RestartSettingsPresenter(SettingsParameterWidget widget, ButtonWidget buttonWidget, Sprite sprite)
		{
			widget.SettingsNameTxt.text = "Restart";
			widget.ButtonsHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 120f);
			var restartButton = Object.Instantiate(buttonWidget, widget.ButtonsHolder);
			restartButton.IconImg.sprite = sprite;
			RestartButton = restartButton.Button;
			_disposable = RestartButton.OnClickAsObservable().Subscribe(OnClick);
		}
		
		private void OnClick(Unit obj)
		{
			_conversationController.Show("Restart Scene", ReloadScene);
		}
		
		private void ReloadScene()
		{
			_projectState.ChangeStateAsync<ProjectLoadSceneState, string>(SceneManager.GetActiveScene().name);
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}