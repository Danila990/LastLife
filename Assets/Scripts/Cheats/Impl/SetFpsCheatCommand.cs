using UnityEngine;

namespace Cheats.Impl
{
	public class SetFpsCheatCommand : ICheatCommandProvider
	{
		public string ButtonText => "HD";
		public bool IsToggle => true;
		
		public void Execute(bool status)
		{
			Application.targetFrameRate = (status ? 60 : 30);
			QualitySettings.SetQualityLevel(status ? 4 : 0);
		}
	}

}