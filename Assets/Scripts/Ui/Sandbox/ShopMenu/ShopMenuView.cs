using System;
using System.Collections.Generic;
using System.Linq;
using Adv.Messages;
using Adv.Services;
using Adv.Services.Interfaces;
using Core.Equipment.Data;
using Core.Factory.Ui;
using Core.ResourcesSystem;
using Core.Services;
using Db.ObjectData;
using Db.Panel;
using MessagePipe;
using Shop;
using Shop.Abstract;
using Shop.ItemProvider;
using TMPro;
using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.ShopMenu.Presenters;
using Ui.Widget;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using VContainerUi.Messages;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.ShopMenu
{
	public class ShopMenuView : SelectMenuMainView
	{
		public NamedButtonWidget SelectPanelButtonWidget;
		public Transform SelectPanelButtonsHolder;

		public ShopButtonWidget ShopButtonWidget;
		public BundleButtonWidget ShopBundleWidget;
		public ShopButtonWidget FreeWidget;
		public ShopButtonWidget NoAdsWidget;
		
		public Transform PanelContentPrefab;
		public Transform BundleContentPrefab;
		public Transform ElementsContentHolder;
		public Button RestorePurchases;
		public TextMeshProUGUI PurchaseTicketReason;
	}

	public class ShopMenuController : SelectMenuController<ShopMenuView>, IStartable, IDisposable
	{
		private readonly IMenuPanelService _menuPanelService;
		private readonly IPanelsData _panelsData;
		private readonly IShopData _shopData;
		private readonly IShopPresentersFactory _shopPresentersFactory;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly ISubscriber<ShowShopMenu> _subscriber;
		private readonly IPublisher<MessageBackWindow> _backWindow;
		private readonly IAPManager _iapManager;
		private readonly IRemoveAdsService _removeAdsService;
		
		private readonly SortedList<string, SelectShopPanelPresenter> _panelPresenters = new SortedList<string, SelectShopPanelPresenter>();
		private readonly SortedList<string, ItemPurchasePresenter> _purchasableItems = new SortedList<string, ItemPurchasePresenter>();
		private readonly List<BundlePurchasePresenter> _bundlePurchase = new List<BundlePurchasePresenter>();
		private readonly List<PurchaseElementPresenter> _presenters = new List<PurchaseElementPresenter>(25);
		
		private SelectShopPanelPresenter _selectedPanel;
		private readonly CompositeDisposable _compositeDisposable = new();
		private bool _openedFromMessage;
		private string _lastOpenedWindow;

		public ShopMenuController(
			IMenuPanelService menuPanelService,
			IPanelsData panelsData,
			IShopData shopData,
			IShopPresentersFactory shopPresentersFactory,
			IItemUnlockService itemUnlockService,
			ISubscriber<ShowShopMenu> subscriber,
			IPublisher<MessageBackWindow> backWindow,
			IAPManager iapManager,
			IRemoveAdsService removeAdsService
		) : base(menuPanelService)
		{
			_menuPanelService = menuPanelService;
			_panelsData = panelsData;
			_shopData = shopData;
			_shopPresentersFactory = shopPresentersFactory;
			_itemUnlockService = itemUnlockService;
			_subscriber = subscriber;
			_backWindow = backWindow;
			_iapManager = iapManager;
			_removeAdsService = removeAdsService;
		}
		
		public void Start()
		{
			CreateTicketPanel();
			CreateShootersPanel();
			CreateEquipmentPanel();
			CreateBoostPanel();
			CreateBundlePanel();
			
			_itemUnlockService.OnItemUnlock.Subscribe(OnItemUnlock).AddTo(View);
			
			var firstId =
				_panelsData.FirstOpenPanel == null
					? _panelPresenters.Values.First().ShopPanelData.PanelId
					: _panelPresenters.Values.First(x => x.ShopPanelData.PanelId == _panelsData.FirstOpenPanel.PanelId).ShopPanelData.PanelId;
			
			SelectPanel(firstId);
			_subscriber.Subscribe(OnOpenShop).AddTo(_compositeDisposable);
			_removeAdsService.RemoveAdsBoughtObservable.Subscribe(OnRemoveAdsBought).AddTo(_compositeDisposable);
			
#if UNITY_IPHONE
			View.RestorePurchases.OnClickAsObservable().Subscribe(RestorePurchases).AddTo(_compositeDisposable);
#else 
			View.RestorePurchases.gameObject.SetActive(false);
#endif
		}
		
		private void CreateBundlePanel()
		{
			var panelPresenter = CreatePanel(_panelsData.BundlesPanel, View.BundleContentPrefab);
			foreach (var bundle in _shopData.BundleScriptable)
			{
				var widget = CreateBundleWidget(panelPresenter.PanelsContent);
				var newPresenter = _shopPresentersFactory.CreateBundlePresenter(widget, bundle.Model);
				if (_itemUnlockService.BundleIsUnlocked(bundle.Model))
				{
					newPresenter.Disable();
				}
				_presenters.Add(newPresenter);
				_bundlePurchase.Add(newPresenter);
			}
			panelPresenter.Hide();
		}

		private void OnRemoveAdsBought(bool isBought)
		{
			if (isBought)
			{
				foreach (var value in _presenters)
				{
					if (value.Model is IRemoveAdsModel { ConstantlyRemoveAds: true, IsRemoveAds: true })
					{
						value.Disable();
					}
				}
			}
		}
		
#if UNITY_IPHONE
		private void RestorePurchases(Unit _)
		{
			Debug.Log("Try restore purchases");
			_iapManager.RestorePurchases();
		}
#endif
		
		private void OnOpenShop(ShowShopMenu shopMenu)
		{
			_openedFromMessage = true;
			_lastOpenedWindow = _menuPanelService.OpenedWindow.Value;
			
			_menuPanelService.SelectMenu(nameof(ShopMenuWindow), shopMenu.CanClose);
			if (_menuPanelService.OpenedWindow.Value != nameof(ShopMenuWindow))
				return;
			
			if (_lastOpenedWindow == nameof(ShopMenuWindow))
			{
				_lastOpenedWindow = string.Empty;
			}
			
			SelectPanel(shopMenu.ID);
			
			if (shopMenu.ForceUseMenuPanel)
				_openedFromMessage = false;
			
			if(!string.IsNullOrEmpty(shopMenu.LastDropId))
				SelectItem(shopMenu.LastDropId);
		}
		
		protected override void HideSelectMenuWindow(Unit obj)
		{
			if (_openedFromMessage)
			{
				if (string.IsNullOrEmpty(_lastOpenedWindow))
				{
					_backWindow.BackWindow();
					_menuPanelService.ClearOpenedWindow();
				}
				else
				{
					_menuPanelService.SelectMenu(_lastOpenedWindow);
				}
			}
			else
			{
				base.HideSelectMenuWindow(obj);
			}
		}

		public override void OnHide()
		{
			base.OnHide();
			_openedFromMessage = false;
			_lastOpenedWindow = string.Empty;
		}

		private void OnItemUnlock(ObjectData obj)
		{
			if (_purchasableItems.TryGetValue(obj.Id, out var item))
			{
				item.Disable();
			}
			
			foreach (var value in _bundlePurchase)
			{
				if (_itemUnlockService.BundleIsUnlocked(value.Model))
				{
					value.Disable();
				}
			}
		}
		
		private void CreateBoostPanel()
		{
			var panelPresenter = CreatePanel(_panelsData.BoostsPanel, View.PanelContentPrefab);
			foreach (var boostObject in _shopData.BoostsItems)
			{
				var widget = CreateWidget(panelPresenter.PanelsContent);
				var newPresenter = _shopPresentersFactory.CreateItemPurchasePresenter(widget, boostObject.Model);
				_presenters.Add(newPresenter);
			}
			panelPresenter.Hide();
		}
		
		private void CreateEquipmentPanel()
		{
			var panelPresenter = CreatePanel(_panelsData.EquipmentPanel, View.PanelContentPrefab);
			
			CreateItemsForCollection(panelPresenter, _shopData.ShopBootsItems);
			CreateItemsForCollection(panelPresenter, _shopData.ShopHatItems);
			CreateItemsForCollection(panelPresenter, _shopData.ShopBulletproofItems);
			CreateItemsForCollection(panelPresenter, _shopData.ShopJetpackItems);

			panelPresenter.Hide();
		}
		
		private void CreateItemsForCollection<T>(SelectShopPanelPresenter panelPresenter, IReadOnlyList<ScriptableEquipmentShopItem<T>> items) 
			where T : EquipmentItemData
		{
			foreach (var equipment in items)
			{
				var widget = CreateWidget(panelPresenter.PanelsContent);
				var newPresenter = _shopPresentersFactory.CreateItemPurchasePresenter(widget, equipment.Model);
				if (_itemUnlockService.IsUnlocked(equipment.Model.Item.ObjectData) ||
				    _itemUnlockService.IsPurchased(equipment.Model))
				{
					newPresenter.Disable();
				}
				_purchasableItems.Add(equipment.ItemId, newPresenter);
			}
		}

		private void CreateTicketPanel()
		{
			var panelPresenter = CreatePanel(_panelsData.TicketsShopPanel, View.PanelContentPrefab);
			//Object.Instantiate(View.PurchaseTicketReason, panelPresenter.PanelsContent);
			foreach (var ticketShopItem in _shopData.TicketShopItems.Where(item => item.Model.ResourceType == ResourceType.Ticket))
			{
				var widget = CreateWidget(panelPresenter.PanelsContent);
				var newPresenter = _shopPresentersFactory.CreateTicketPresenter(widget, ticketShopItem.Model);
			}
			
			var noAdsWidget = Object.Instantiate(View.NoAdsWidget, panelPresenter.PanelsContent);
			var noAdsPresenter = _shopPresentersFactory.CreateNoAdsPresenter(noAdsWidget, _shopData.RemoveAdsModel.Model);
			_presenters.Add(noAdsPresenter);

			var freeWidget = Object.Instantiate(View.FreeWidget, panelPresenter.PanelsContent);
			var presenter = _shopPresentersFactory.CreateFreeTicketPresenter(freeWidget);

			foreach (var ticketShopItem in _shopData.TicketShopItems.Where(item => item.Model.ResourceType == ResourceType.GoldTicket))
			{
				var widget = CreateWidget(panelPresenter.PanelsContent);
				var newPresenter = _shopPresentersFactory.CreateTicketPresenter(widget, ticketShopItem.Model);
			}
			
			panelPresenter.Hide();
		}
		
		private void CreateShootersPanel()
		{
			var panelPresenter = CreatePanel(_panelsData.ShootersPanel, View.PanelContentPrefab);
			foreach (var shopDataCharacter in _shopData.CharacterShopItems)
			{
				var widget = CreateWidget(panelPresenter.PanelsContent);
				var newPresenter = _shopPresentersFactory.CreateItemPurchasePresenter(widget, shopDataCharacter.Model);
				if (_itemUnlockService.IsUnlocked(shopDataCharacter.Model.Item.ObjectData) ||
				    _itemUnlockService.IsPurchased(shopDataCharacter.Model))
				{
					newPresenter.Disable();
				}
				_purchasableItems.Add(shopDataCharacter.ItemId, newPresenter);
				_presenters.Add(newPresenter);
			}
			panelPresenter.Hide();
		}
		
		private SelectShopPanelPresenter CreatePanel(IShopPanelData shopPanel, Transform contentPrefab)
		{
			var widget = Object.Instantiate(View.SelectPanelButtonWidget, View.SelectPanelButtonsHolder);
			var panelsContent = Object.Instantiate(contentPrefab, View.ElementsContentHolder);
				
			var panelPresenter = new SelectShopPanelPresenter(this, widget, shopPanel, panelsContent).AddTo(View);
			_panelPresenters.Add(shopPanel.PanelId, panelPresenter);

			return panelPresenter;
		}

		private void SelectItem(string id)
		{
			foreach (var item in _purchasableItems)
			{
				if (item.Key.Equals(id, StringComparison.InvariantCultureIgnoreCase))
				{
					item.Value.Highlight();
					return;
				}
			}
		}
		
		public void SelectPanel(string shopPanelPanelId)
		{
			var selectShopPanelPresenter = _panelPresenters[shopPanelPanelId];
			if (_selectedPanel == selectShopPanelPresenter)
				return;
			
			_selectedPanel?.Hide();
			_selectedPanel = selectShopPanelPresenter;
			_selectedPanel.Select();
		}
		
		private ShopButtonWidget CreateWidget(Transform panelPresenter) 
			=> Object.Instantiate(View.ShopButtonWidget, panelPresenter);

		private BundleButtonWidget CreateBundleWidget(Transform panelPresenter) 
			=> Object.Instantiate(View.ShopBundleWidget, panelPresenter);
		
		public void Dispose()
		{
			_selectedPanel?.Dispose();
			_compositeDisposable?.Dispose();
		}
	}
}