using System.Collections.Generic;
using Core.Quests.Tips;
using Core.Quests.Tips.Impl;
using Core.Services;
using UniRx;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.SelectMenuButtons
{
	public class SelectMenuButtonsView : UiView
	{
		public WindowButtonElement[] WindowButtonElements;
		public QuestTipContext[] Tips;
	}

	public class SelectMenuButtonsController : UiController<SelectMenuButtonsView>, IStartable
	{
		private readonly IMenuPanelService _menuPanelService;
		private readonly IQuestTipService _questTipService;
		private readonly Dictionary<string, WindowButtonElement> _windowButtons = new Dictionary<string, WindowButtonElement>();
		private WindowButtonElement _selectedWindow;

		public SelectMenuButtonsController(
			IMenuPanelService menuPanelService,
			IQuestTipService questTipService
			)
		{
			_menuPanelService = menuPanelService;
			_questTipService = questTipService;
		}
		
		public void Start()
		{
			foreach (var windowButton in View.WindowButtonElements)
			{
				windowButton.Init();
				windowButton.OnClick += OnClickWindowButton;
				_windowButtons.Add(windowButton.MenuName, windowButton);
			}

			foreach (var tip in View.Tips)
			{
				foreach (var inlineId in tip.QuestInlineIds)
					_questTipService.AddTip(tip, inlineId);
			}

			_menuPanelService.OpenedWindow.Subscribe(OnSelectedWindowChange);
		}
		
		private void OnSelectedWindowChange(string openedPanel)
		{
			if (_selectedWindow && _selectedWindow.MenuName == openedPanel)
				return;
			
			if(_selectedWindow)
				_selectedWindow.DeselectOutline();
			
			if (string.IsNullOrEmpty(openedPanel))
			{
				_selectedWindow = null;
				return;
			}
			if (_windowButtons.TryGetValue(openedPanel, out var windowButton))
			{
				_selectedWindow = windowButton;
				_selectedWindow.SelectOutline();
			}
		}

		private void OnClickWindowButton(WindowButtonElement buttonElement)
		{
			_menuPanelService.SelectMenu(buttonElement.MenuName);
		}
	}
}