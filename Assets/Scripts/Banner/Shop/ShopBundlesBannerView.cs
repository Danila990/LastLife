using System;
using System.Linq;
using Core.Factory.Ui;
using Core.Services;
using MessagePipe;
using SharedUtils;
using SharedUtils.PlayerPrefs;
using SharedUtils.PlayerPrefs.Impl;
using Shop.Abstract;
using Ui.Widget;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainerUi.Abstraction;
using VContainerUi.Messages;

namespace Banner.Shop
{
	public class ShopBundlesBannerView : UiView
	{
		public RectTransform BannerContent;
		public BundleButtonWidget ShopBundleWidget;
		public Button CloseButton;
	}

	public class ShopBundlesBannerUiController : UiController<ShopBundlesBannerView>, IBannerController, IDisposable
	{
		private readonly IPublisher<MessageBackWindow> _backWindow;
		private readonly IPublisher<MessageOpenWindow> _openWindow;
		private readonly IShopData _shopData;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IShopPresentersFactory _shopPresentersFactory;
		private readonly InMemoryPlayerPrefsManager _inMemoryPlayerPrefsManager;
		private CompositeDisposable _compositeDisposable;
		
		private ReactiveCommand _hideCommand;
		public IReactiveCommand<Unit> Hide => _hideCommand;
		
		public ShopBundlesBannerUiController(
			IPublisher<MessageBackWindow> backWindow,
			IPublisher<MessageOpenWindow> openWindow, 
			IShopData shopData,
			IItemUnlockService itemUnlockService,
			IShopPresentersFactory shopPresentersFactory, 
			InMemoryPlayerPrefsManager inMemoryPlayerPrefsManager)
		{
			_backWindow = backWindow;
			_openWindow = openWindow;
			_shopData = shopData;
			_itemUnlockService = itemUnlockService;
			_shopPresentersFactory = shopPresentersFactory;
			_inMemoryPlayerPrefsManager = inMemoryPlayerPrefsManager;
		}
				
		public bool IsAvailable()
		{
			return _inMemoryPlayerPrefsManager.GetValue("MetroUsed", false) && _shopData.BundleScriptable.Any(item => !_itemUnlockService.BundleIsUnlocked(item.Model));
		}
		
		public void ShowBanner()
		{
			_openWindow.OpenWindow<BundlesBannersWindow>();
		}

		public override void OnShow()
		{
			_compositeDisposable?.Dispose();
			_compositeDisposable = new CompositeDisposable();
			_hideCommand = new ReactiveCommand().AddTo(_compositeDisposable);
			
			var randomBundle = _shopData.BundleScriptable.Where(item => !_itemUnlockService.BundleIsUnlocked(item.Model)).GetRandomOrDefault();
			if (randomBundle == null)
			{
				Debug.LogError("Bundle is not available");
				Close(default(Unit));
				return;
			}
			var bundlePresenter = _shopPresentersFactory.CreateBundlePresenter(View.ShopBundleWidget, randomBundle.Model);
			View.CloseButton.OnClickAsObservable().Subscribe(Close).AddTo(_compositeDisposable);
			_itemUnlockService.OnItemUnlock.Subscribe(_ => Close(default(Unit))).AddTo(_compositeDisposable);
		}
		
		private void Close(Unit obj)
		{
			_hideCommand.Execute();
			_compositeDisposable?.Dispose();
			_backWindow.BackWindow();
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}