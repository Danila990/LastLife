using System;
using Core.Services;
using GameSettings;
using Sirenix.OdinInspector;
using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.WorldSpaceUI;
using Ui.Widget;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SettingsMenu
{
	public class SettingsMenuView : SelectMenuMainView
	{
		public SettingsParameterWidget ParameterWidgetPrefab;
		public NamedButtonWidget DefaultTxtButtonWidget;
		public ButtonWidget ImageOnlyBtnWidget;
		public WorldSpaceSupplyBox GenericAdvTicketWidget;
		[BoxGroup("Restart")] public Sprite RestartIcon; 
		
		[BoxGroup("Clean")] public Sprite ClearIcon;
		[BoxGroup("Clean")] public Sprite CharacterIcon;
			
		public RectTransform ParametersHolder;
	}

	public class SettingsMenuUiController : SelectMenuController<SettingsMenuView>, IStartable, IDisposable
	{
		private readonly ISettingsService _settingsService;
		private readonly IObjectResolver _resolver;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

		public ClearSettingsPresenter ClearSettingsPresenter { get; private set; }
		public RestartSettingsPresenter RestartSettingsPresenter { get; private set; }

		public SettingsMenuUiController(
			IMenuPanelService menuPanelService,
			ISettingsService settingsService, 
			IObjectResolver resolver) 
			: base(menuPanelService)
		{
			_settingsService = settingsService;
			_resolver = resolver;
		}
		
		public void Start()
		{
			CreateParameterForPreset(_settingsService.QualityPreset);
			ClearSetting();
#if !RELEASE_BRANCH
			CreateRestartSetting();
#endif
		}

		private void ClearSetting()
		{
			var widget = CreateSettingsWidget();
			ClearSettingsPresenter = new ClearSettingsPresenter(widget, View.GenericAdvTicketWidget, View.ClearIcon, View.CharacterIcon).AddTo(_compositeDisposable);
			_resolver.Inject(ClearSettingsPresenter);
		}
		
		private void CreateRestartSetting()
		{
			var widget = CreateSettingsWidget();
			RestartSettingsPresenter = new RestartSettingsPresenter(widget, View.ImageOnlyBtnWidget, View.RestartIcon).AddTo(_compositeDisposable);
			_resolver.Inject(RestartSettingsPresenter);
		}

		private void CreateParameterForPreset(SettingsPreset settingsServiceQualityPreset)
		{
			var widget = CreateSettingsWidget();
			new SettingPresetPresenter(widget, settingsServiceQualityPreset, View.DefaultTxtButtonWidget).AddTo(_compositeDisposable);
		}
		
		private SettingsParameterWidget CreateSettingsWidget()
		{
			var widget = Object.Instantiate(View.ParameterWidgetPrefab, View.ParametersHolder);
			return widget;
		}

		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}