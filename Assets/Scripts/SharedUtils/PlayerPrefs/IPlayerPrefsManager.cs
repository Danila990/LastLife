namespace SharedUtils.PlayerPrefs
{
	public interface IPlayerPrefsManager
	{
		/// <summary>
		/// Accepted Types: typeof(int), typeof(bool), typeof(float), typeof(long), typeof(string)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <typeparam name="T"></typeparam>
		void SetValue<T>(string key, T val);
		T GetValue<T>(string key);
		T GetValue<T>(string key, T defaultValue);
        
		bool HasKey(string key);
		void DeleteKey(string key);
		void DeleteAll();
		void Save();
	}
}