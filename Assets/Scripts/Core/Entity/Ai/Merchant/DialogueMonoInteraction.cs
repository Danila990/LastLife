using Core.Entity.Characters;
using Core.Entity.InteractionLogic.Interactions;
using Core.Quests.Messages;
using Dialogue.Services;
using Dialogue.Services.Interfaces;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.Ai.Merchant
{
	public class DialogueMonoInteraction : ItemSupplyInteraction
	{
		[SerializeField, BoxGroup("DialogueOptions")] private MonoModuleDialogueArgs[] _dialogueArgs;
		[SerializeField, BoxGroup("DialogueOptions")] private string _dialoguePlaceId;
		[SerializeField, BoxGroup("DialogueOptions")] private string _merchantId;
		
		private IDialogueService _dialogueService;
		
		private IDialogueSource _dialogueSource;
		private DialogueOptions _cashedOptions;
		public IDialogueSource DialogueSource => _dialogueSource;

		[Inject]
		private void Construct(IObjectResolver resolver)
		{
			_dialogueService = resolver.Resolve<IDialogueService>();
		}

		private void Awake()
		{
			_dialogueSource = GetComponent<IDialogueSource>();
		}

		protected override void Select(CharacterContext characterContext)
		{
			var nodeName = _dialogueSource.GetDialogueNodeName();
			_cashedOptions ??= new DialogueOptions(_dialogueArgs, _dialoguePlaceId, _dialogueSource);
			_dialogueService.StartDialogue(nodeName, _cashedOptions, _merchantId);
		}

		public DialogueOptions GetOptions()
		{
			return new DialogueOptions(_dialogueArgs, _dialoguePlaceId, _dialogueSource);
		}

		protected override void ShowUI()
		{
			base.ShowUI();
			CurrentUI.Button.GetComponentInChildren<TextMeshProUGUI>().text = "Speak";
		}

		protected override void DisableUI()
		{
			if (CurrentUI)
			{
				CurrentUI.Button.GetComponentInChildren<TextMeshProUGUI>().text = "Use";
			}
			base.DisableUI();
		}
	}
}