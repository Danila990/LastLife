using Dialogue.Services.Interfaces;
using SharedUtils.PlayerPrefs;
using Yarn.Unity;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class PlayerPrefsModule : IDialogueModule
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private static PlayerPrefsModule _instance;

		public PlayerPrefsModule(
			IPlayerPrefsManager playerPrefsManager
		)
		{
			_playerPrefsManager = playerPrefsManager;
			_instance = this;
		}
		public string ModuleId => nameof(PlayerPrefsModule);
		
		public void OnStartDialogue(IModuleArgs moduleArgs = null)
		{
		}
		public void OnDialogueEnd()
		{
			
		}
		
		public void AddCommand(DialogueRunner dialogueRunner)
		{
		}

		[YarnFunction("getBool")]
		public static bool GetBoolByKey(string key, bool defaultValue)
		{
			return _instance._playerPrefsManager.GetValue<bool>(key, defaultValue);
		}
		
		[YarnCommand("setBool")]
		public static void SetBoolByKey(string key, bool value)
		{
			_instance._playerPrefsManager.SetValue<bool>(key, value);
		}
	}
}