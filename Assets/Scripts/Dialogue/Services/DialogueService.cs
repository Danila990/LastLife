using System.Collections.Generic;
using System.Linq;
using Core.Quests.Messages;
using Dialogue.Services.Interfaces;
using Dialogue.Ui;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer.Unity;
using VContainerUi.Messages;
using VContainerUi.Services;
using Yarn.Unity;

namespace Dialogue.Services
{
	public interface IDialogueService
	{
		DialogueRunner DialogueRunner { get; }
		void StartDialogue(string nodeName, DialogueOptions dialogueOptions = null, string merchantId = null);
		void StopDialogueForce();
	}
	
	public class DialogueService : IDialogueService, IInitializable
	{
		private readonly IUiMessagesPublisherService _uiMessagesPublisherService;
		private readonly List<IDialogueModule> _activeModules = new List<IDialogueModule>();
		private readonly Dictionary<string, IDialogueModule> _modules;
		private readonly IQuestMessageSender _questMessageSender;
		private string _merchantId;

		public DialogueRunner DialogueRunner { get; }
		public YarnProject YarnProject { get; }

		public DialogueService(
			DialogueRunner dialogueRunner, 
			YarnProject yarnProject, 
			IUiMessagesPublisherService uiMessagesPublisherService,
			IQuestMessageSender questMessageSender,
			IEnumerable<IDialogueModule> modules)
		{
			_uiMessagesPublisherService = uiMessagesPublisherService;
			_modules = modules.ToDictionary(module => module.ModuleId);
			DialogueRunner = dialogueRunner;
			YarnProject = yarnProject;
			_questMessageSender = questMessageSender;

		}
		
		public void Initialize()
		{
			DialogueRunner.SetProject(YarnProject);
			DialogueRunner.onDialogueComplete.AddListener(OnDialogueEnd);
			
			foreach (var (key, value) in _modules)
			{
				value.AddCommand(DialogueRunner);
			}
		}

		[Button]
		public void StartDialogue(string nodeName, DialogueOptions dialogueOptions = null, string merchantId = null)
		{
			_merchantId = merchantId;
			PrepareModules(dialogueOptions);
			_uiMessagesPublisherService.OpenWindowPublisher.OpenWindow<DialogueWindow>();
			DialogueRunner.StartDialogue(nodeName);
		}
		
		[Button]
		public void StopDialogueForce()
		{
			foreach (var dialogueRunnerDialogueView in DialogueRunner.dialogueViews)
			{
				var canvasGroup = dialogueRunnerDialogueView.GetComponent<CanvasGroup>();
				canvasGroup.blocksRaycasts = false;
				canvasGroup.interactable = false;
				canvasGroup.alpha = 0;
				dialogueRunnerDialogueView.DialogueComplete();
			}
			DialogueRunner.Stop();
		}
		
		private void PrepareModules(DialogueOptions dialogueOptions)
		{
			if (dialogueOptions is null)
				return;
			
			_activeModules.Clear();
			foreach (var moduleArg in dialogueOptions.ModuleArgs)
			{
				if (_modules.TryGetValue(moduleArg.ModuleId, out var module))
				{
					module.OnStartDialogue(moduleArg);
					_activeModules.Add(module);
				}
			}
		}
		
		private void OnDialogueEnd()
		{
			foreach (var module in _activeModules)
			{
				module.OnDialogueEnd();
			}
			if(!string.IsNullOrEmpty(_merchantId))
				_questMessageSender.SendTalkMerchantMessage(_merchantId);
			
			_uiMessagesPublisherService.BackWindowPublisher.BackWindow();
			_merchantId = null;
		}
	}

	public class DialogueOptions
	{
		public IDialogueSource DialogueSource;
		public string DialoguePlaceId;
		public IModuleArgs[] ModuleArgs;
		
		public DialogueOptions(IModuleArgs[] moduleArgs, string dialoguePlaceId, IDialogueSource dialogueSource)
		{
			ModuleArgs = moduleArgs;
			DialoguePlaceId = dialoguePlaceId;
			DialogueSource = dialogueSource;
		}
	}
}