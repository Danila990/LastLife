using SharedUtils.PlayerPrefs;
using UnityEngine;

namespace Cheats.Impl
{
	public class ClearPrefsCheatCommand : ICheatCommandProvider
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		public string ButtonText => "clear \n cache";
		public bool IsToggle => false;

		public ClearPrefsCheatCommand(IPlayerPrefsManager playerPrefsManager)
		{
			_playerPrefsManager = playerPrefsManager;
		}
		
		public void Execute(bool status)
		{
			var curPass = _playerPrefsManager.GetValue<string>(CheatPanelController.KEY_PASSWORD, "");
			PlayerPrefs.DeleteAll();
			_playerPrefsManager.SetValue<string>(CheatPanelController.KEY_PASSWORD, curPass);
		}
	}
}