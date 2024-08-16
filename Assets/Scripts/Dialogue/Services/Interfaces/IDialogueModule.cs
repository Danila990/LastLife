using Yarn.Unity;

namespace Dialogue.Services.Interfaces
{
	public interface IDialogueModule
	{
		string ModuleId { get; }
		
		void OnStartDialogue(IModuleArgs moduleArgs = null);
		void OnDialogueEnd();
		
		void AddCommand(DialogueRunner dialogueRunner);
	}
}