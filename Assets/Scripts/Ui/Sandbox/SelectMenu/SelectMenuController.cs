using System;
using System.Collections.Generic;
using Core.Services;
using UniRx;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.SelectMenu
{
	public abstract class SelectMenuController<T> : UiController<T>
		where T : SelectMenuMainView
	{
		private readonly IMenuPanelService _menuPanelService;
		private CompositeDisposable _showOnlyDisposable = new CompositeDisposable();
		protected ICollection<IDisposable> ShowOnlyDisposable => _showOnlyDisposable;
		protected IMenuPanelService MenuPanelService => _menuPanelService;
		public SelectMenuController(IMenuPanelService menuPanelService)
		{
			_menuPanelService = menuPanelService;
		}
		
		public override void OnShow()
		{
			_showOnlyDisposable = new CompositeDisposable();
			View.HideButton.OnClickAsObservable().Subscribe(HideSelectMenuWindow).AddTo(_showOnlyDisposable);
		}

		public override void OnHide()
		{
			_showOnlyDisposable.Dispose();
		}

		protected virtual void HideSelectMenuWindow(Unit obj)
		{
			_menuPanelService.CloseMenu();
		}
	}
}