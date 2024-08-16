namespace Cheats
{
	public interface ICheatCommandProvider
	{
		string ButtonText { get; }
		bool IsToggle { get; }
		void Execute(bool status);
	}
}