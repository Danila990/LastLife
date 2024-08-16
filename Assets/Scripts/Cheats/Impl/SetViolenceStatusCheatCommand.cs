namespace Cheats.Impl
{
	public class SetViolenceStatusCheatCommand : ICheatCommandProvider
	{
		public string ButtonText => "No Violence";
		public bool IsToggle => true;
		
		public void Execute(bool status)
		{
			GameSettings.GameSetting.ViolenceStatus = status;
		}
	}

}