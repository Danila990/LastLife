using System;
using System.Collections.Generic;
using GameSettings;
using Ui.Widget;
using UniRx;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SettingsMenu
{
	public class SettingPresetPresenter : IDisposable
	{
		private readonly SettingsParameterWidget _widget;
		private readonly SettingsPreset _settingsServiceQualityPreset;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly List<NamedButtonWidget> _buttonWidgets = new List<NamedButtonWidget>();
		
		public SettingPresetPresenter(SettingsParameterWidget widget, SettingsPreset settingsServiceQualityPreset, NamedButtonWidget buttonWidgetPrefab)
		{
			_widget = widget;
			_settingsServiceQualityPreset = settingsServiceQualityPreset;

			_widget.SettingsNameTxt.text = settingsServiceQualityPreset.PresetName;

			for (var index = 0; index < settingsServiceQualityPreset.Presets.Length; index++)
			{
				var setting = settingsServiceQualityPreset.Presets[index];
				var buttonWidget = Object.Instantiate(buttonWidgetPrefab, _widget.ButtonsHolder);
				buttonWidget.Text.text = setting.SettingNormalName;
				buttonWidget.Button.OnClickAsObservable().SubscribeWithState(index, OnClickPreset).AddTo(_compositeDisposable);
				if (_settingsServiceQualityPreset.SelectedPreset.Value.SettingNormalName == setting.SettingNormalName)
				{
					buttonWidget.Button.interactable = false;
				}
				_buttonWidgets.Add(buttonWidget);
			}
			
		}
		
		private void OnClickPreset(Unit arg1, int newPresetIndex)
		{
			foreach (var namedButtonWidget in _buttonWidgets)
				namedButtonWidget.Button.interactable = true;
			
			_buttonWidgets[newPresetIndex].Button.interactable = false;
			_settingsServiceQualityPreset.SelectPreset(newPresetIndex);
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}