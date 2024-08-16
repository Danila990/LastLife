using UnityEngine;

namespace Cheats.Impl
{
	public class CloseUiCheatCommand : ICheatCommandProvider
	{
		public string ButtonText => "hide \n ui";
		public bool IsToggle => true;
		
		public void Execute(bool status)
		{
			var groups = Object.FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (var group in groups)
			{
				group.alpha = status ? 0 : 1f;
			}
		}
	}

}