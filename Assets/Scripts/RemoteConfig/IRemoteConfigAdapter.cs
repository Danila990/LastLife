using System;

namespace RemoteConfig
{
	public interface IRemoteConfigAdapter
	{
		IObservable<bool> OnConfigUpdated { get; }
		string GetValue(string key, string defaultValue);
	}
}