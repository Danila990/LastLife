using System;
using RemoteConfig;
using SharedUtils.PlayerPrefs;
using UniRx;
using UnityEngine.Pool;
using VContainer.Unity;

namespace GameSettings
{
	public interface ISettingsService
	{
		SettingsPreset QualityPreset { get; }
		T GetValue<T>(string key, GameSetting.ParameterType parameterType);
		IObservable<string> OnValueByKeyChanged { get; }
	}
	
	public class SettingsService : IInitializable, ISettingsService, IDisposable
	{
		private readonly SettingsProvider _settingsProvider;
		private readonly IRemoteConfigAdapter _remoteConfigAdapter;
		private readonly IPlayerPrefsManager _prefsManager;

		private readonly ReactiveCommand<string> _onValueByKeyChanged = new ReactiveCommand<string>();

		public IObservable<string> OnValueByKeyChanged => _onValueByKeyChanged;
		public SettingsPreset QualityPreset => _settingsProvider.QualitySetting;

		private IDisposable _disposable;

		public SettingsService(
			SettingsProvider settingsProvider,
			IRemoteConfigAdapter remoteConfigAdapter,
			IPlayerPrefsManager prefsManager)
		{
			_settingsProvider = settingsProvider;
			_remoteConfigAdapter = remoteConfigAdapter;
			_prefsManager = prefsManager;
		}
		
		public void Initialize()
		{
			_settingsProvider.Initialize();
			QualityPreset.Initialize(_prefsManager);
			_disposable = _remoteConfigAdapter.OnConfigUpdated.Subscribe(OnConfigLoaded);
		}

		public T GetValue<T>(string key, GameSetting.ParameterType parameterType)
		{
			return _settingsProvider.MainSetting.GetValue<T>(key, parameterType);
		}
		
		public void SetValue<T>(string key, T value, GameSetting.ParameterType parameterType)
		{
			_settingsProvider.MainSetting.OverrideValue<T>(key, parameterType, value);
			_onValueByKeyChanged.Execute(key);
		}
		
		private void OnConfigLoaded(bool status)
		{
			if (!status)
				return;
			
			var dictionary = _settingsProvider.MainSetting.GetDictionary<string>(GameSetting.ParameterType.String);
			var pool = ListPool<(string, string)>.Get();
			
			foreach (var kvp in dictionary)
			{
				var newValue = _remoteConfigAdapter.GetValue(kvp.Key, kvp.Value);
				if (newValue != kvp.Value)
				{
					pool.Add((kvp.Key, newValue));
				}
			}

			foreach (var kvp in pool)
			{
				dictionary[kvp.Item1] = kvp.Item2;
				_onValueByKeyChanged.Execute(kvp.Item1);
			}
			
			ListPool<(string, string)>.Release(pool);
		}

		public void Dispose()
		{
			_onValueByKeyChanged?.Dispose();
			_disposable?.Dispose();
		}
	}
}