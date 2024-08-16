using System;
using System.Linq;
using SharedUtils.PlayerPrefs;
using UniRx;
using UnityEngine;
using Utils;

namespace GameSettings
{
	[Serializable]
	public class SettingsPreset
	{
		[field:SerializeField] public string PresetName { get; private set; }
		public string PresetPrefKey;
		public GameSetting[] Presets;
		public int DefaultPresetIndex;
		private IPlayerPrefsManager _playerPrefsManager;
		public ReactiveProperty<GameSetting> SelectedPreset { get; set; }
		
		public void Initialize(IPlayerPrefsManager playerPrefsManager)
		{
			_playerPrefsManager = playerPrefsManager;
			foreach (var preset in Presets)
			{
				preset.Initialize();
			}

			var index = playerPrefsManager.GetValue<int>(PresetPrefKey, DefaultPresetIndex);
			var selected = index.InBounds(Presets) ? Presets[index] : Presets.LastOrDefault();
			if(selected)
				SelectedPreset = new ReactiveProperty<GameSetting>(selected);
		}

		public void SelectPreset(int index)
		{
			SelectedPreset.Value = Presets[index];
			_playerPrefsManager.SetValue(PresetPrefKey, index);
		}
	}
}