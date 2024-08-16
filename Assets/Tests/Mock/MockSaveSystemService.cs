using Core.Services.SaveSystem;

namespace Tests.Mock
{
	public class MockSaveSystemService : ISaveSystemService
	{
		public bool TryGetFromLoadedStorage(string key, out string result)
		{
			result = "";
			return false;
		}
	}
}