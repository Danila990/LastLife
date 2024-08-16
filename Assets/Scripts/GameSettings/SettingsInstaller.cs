using System;
using UnityEngine;
using VContainer;
using VContainer.Extensions;

namespace GameSettings
{
	public class SettingsInstaller : MonoInstaller
	{
		[SerializeField] private SettingsProvider _settingsProvider;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.RegisterInstance(_settingsProvider);
			builder.Register<SettingsService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GraphicSettingListener>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
	
	[Serializable]
	public class SettingsProvider
	{
		public GameSetting MainSetting;
		public SettingsPreset QualitySetting;

		public void Initialize()
		{
			MainSetting.Initialize();
		}
	}
}