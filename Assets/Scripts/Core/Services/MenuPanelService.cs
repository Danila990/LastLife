using System;
using Ui.Sandbox;
using UniRx;
using VContainerUi.Messages;
using VContainerUi.Services;

namespace Core.Services
{
	public interface IMenuPanelService
	{
		IReadOnlyReactiveProperty<string> OpenedWindow { get; }
		void SelectMenu(string menuName);
		void SelectMenu(string menuName, bool canClose);
		void SetBlockSelectingStatus(bool status);
		void CloseMenu();
		void ClearOpenedWindow();
	}
	
	public class MenuPanelService : IMenuPanelService, IDisposable
	{
		private readonly IUiMessagesPublisherService _uiMessagesPublisherService;
		private readonly StringReactiveProperty _currentWindow;
		
		public IReadOnlyReactiveProperty<string> OpenedWindow => _currentWindow;

		public MenuPanelService(
			IUiMessagesPublisherService uiMessagesPublisherService)
		{
			_uiMessagesPublisherService = uiMessagesPublisherService;

			_currentWindow = new StringReactiveProperty();
		}

		public void SelectMenu(string menuName)
		{
			if (_currentWindow.Value != null && _currentWindow.Value == menuName)
			{
				CloseMenu();
			}
			else
			{
				_uiMessagesPublisherService.OpenWindowPublisher.Publish(new MessageOpenWindow(menuName));
				_currentWindow.Value = menuName;
			}
		}
		public void SelectMenu(string menuName, bool canClose)
		{
			if (canClose)
			{
				SelectMenu(menuName);
				return;
			}

			if (string.IsNullOrEmpty(_currentWindow.Value) || _currentWindow.Value != menuName)
			{
				_uiMessagesPublisherService.OpenWindowPublisher.Publish(new MessageOpenWindow(menuName));
				_currentWindow.Value = menuName;
			}
		}

		public void SetBlockSelectingStatus(bool status)
		{
		}

		public void CloseMenu()
		{
			_currentWindow.Value = null;
			_uiMessagesPublisherService.OpenWindowPublisher.OpenWindow<SandboxMainWindow>();
		}
		
		public void ClearOpenedWindow()
		{
			_currentWindow.Value = null;
		}

		public void Dispose()
		{
			_currentWindow.Dispose();	
		}
	}
}