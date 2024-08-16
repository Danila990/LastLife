using System;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer.Unity;

namespace GameSettings
{
	public class GraphicSettingListener : IInitializable, IDisposable
	{
		private readonly ISettingsService _settingsService;
		private IDisposable _disposable;
		
		public GraphicSettingListener(ISettingsService settingsService)
		{
			_settingsService = settingsService;
		}
		
		public void Initialize()
		{
			_disposable = _settingsService.QualityPreset.SelectedPreset.Subscribe(OnSettingChanged);
		}
		
		private void OnSettingChanged(GameSetting setting)
		{
			var isHdMod = setting.GetValue<bool>(SettingsConsts.HD_MOD, GameSetting.ParameterType.Bool);
			SetHdStatus(isHdMod);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
		
		public static void SetFPS(int fps)
		{
			Application.targetFrameRate = fps;
		}
		
		public static void SetHdStatus(bool on)
		{
			SetFPS(on ? 60 : 30);
			QualitySettings.SetQualityLevel(on ? 4 : 0);
		}
	}
}