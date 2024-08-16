using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dialogue.Services;
using Dialogue.Ui.CustomViews;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using VContainerUi.Abstraction;
using Yarn.Unity;

namespace Dialogue.Ui
{
	public class DialogueUiView : UiView
	{
		public Button StopDialogue;
		public CanvasGroup CanvasGroup;
		public PopupWidget PopupWidgetPrefab;
	}

	public class DialogueUiController : UiController<DialogueUiView>, IInitializable
	{
		private readonly DialogueViewBase[] _dialogueViewPrefabs;
		private readonly IDialogueService _dialogueService;
		private DialogueViewBase[] _activeDialogsViews;

		public DialogueRunner DialogueRunner => _dialogueService.DialogueRunner;
		public IEnumerable<string> CurrentNodeTags { get; private set; }
		public string LastNodeName { get; private set; }

		public DialogueUiController(
			DialogueViewBase[] dialogueViewPrefabs, 
			IDialogueService dialogueService)
		{
			_dialogueViewPrefabs = dialogueViewPrefabs;
			_dialogueService = dialogueService;
		}

		public void CreatePopup(string text, Sprite ico)
		{
			var widget = Object.Instantiate(View.PopupWidgetPrefab, View.transform);
			widget.Text.text = text;
			if (ico is null)
			{
				widget.Image.transform.parent.gameObject.SetActive(false);
			}
			else
			{
				widget.Image.sprite = ico;
			}
			AsyncAnimation(widget, View.destroyCancellationToken).Forget();
		}

		private async UniTask AsyncAnimation(PopupWidget widget, CancellationToken token)
		{
			await LMotion
				.Create(widget.RectTransform.anchoredPosition.y, -200f, 1f)
				.WithEase(Ease.InSine)
				.BindToAnchoredPositionY(widget.RectTransform)
				.ToUniTask(cancellationToken: token);

			await UniTask.Delay(1f.ToSec(), cancellationToken: token);
			
			await LMotion
				.Create(widget.RectTransform.anchoredPosition.y, 0, 1f)
				.WithEase(Ease.OutSine)
				.BindToAnchoredPositionY(widget.RectTransform)
				.ToUniTask(cancellationToken: token);

			Object.Destroy(widget.gameObject);
		}
		
		public void Initialize()
		{
			_activeDialogsViews = new DialogueViewBase[_dialogueViewPrefabs.Length];
			for (var index = 0; index < _dialogueViewPrefabs.Length; index++)
			{
				var dialogueView = _dialogueViewPrefabs[index];
				var view = Object.Instantiate(dialogueView, View.transform);
				if (view is ICustomDialogueView customDialogueView)
				{
					customDialogueView.DialogueUiController = this;
				}
				_activeDialogsViews[index] = view;
			}
			
			View.StopDialogue.onClick.AddListener(StopDialogue);
			_dialogueService.DialogueRunner.SetDialogueViews(_activeDialogsViews);
			_dialogueService.DialogueRunner.onNodeStart.AddListener(OnNodeStart);
			_dialogueService.DialogueRunner.onNodeComplete.AddListener(OnNodeComplete);
		}

		public void MoveBackNode()
		{
			_dialogueService.DialogueRunner.Dialogue.SetNode(LastNodeName);
			_dialogueService.DialogueRunner.OnViewRequestedInterrupt();
		}
		
		private void OnNodeComplete(string nodeName)
		{
			LastNodeName = nodeName;
		}

		private void OnNodeStart(string nodeName)
		{ 
			CurrentNodeTags = _dialogueService.DialogueRunner.GetTagsForNode(nodeName);
		}

		private void StopDialogue()
		{
			if(!InFocus)
				return;
			_dialogueService.StopDialogueForce();
		}
	}
}