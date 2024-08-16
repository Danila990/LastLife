using Adv.Services;
using Adv.Services.Interfaces;
using Core.Quests.Messages;
using Core.ResourcesSystem.Interfaces;
using Db.ObjectData;
using Shop;
using Shop.Models;
using Ticket;
using Ui.Sandbox.ShopMenu.Presenters;
using Ui.Widget;

namespace Core.Factory.Ui
{
	public interface IShopPresentersFactory
	{
		TicketPurchasePresenter CreateTicketPresenter(ShopButtonWidget shopButtonWidget, TicketModel model);
		ItemPurchasePresenter CreateItemPurchasePresenter<T>(ShopButtonWidget shopButtonWidget, ShopObjectItemModel<T> model) where T : ObjectData;
		FreeRewardTicketElementPresenter CreateFreeTicketPresenter(ShopButtonWidget shopButtonWidget);
		NoAdsWidgetPresenter<T> CreateNoAdsPresenter<T>(ShopButtonWidget noAdsWidget, T removeAdsModel) where T : ShopItemModel, IRemoveAdsModel;
		BundlePurchasePresenter CreateBundlePresenter(BundleButtonWidget widget, BundleShopItemModel bundleModel);
	}
	
	public class ShopPresentersFactory : IShopPresentersFactory
	{
		private readonly IPurchaseService _purchaseService;
		private readonly ITicketService _ticketService;
		private readonly IAdvService _advService;
		private readonly IRemoveAdsService _removeAdsService;
		private readonly IResourcesService _resourcesService;
		private readonly IQuestMessageSender _questMessageSender;
		private readonly ILocalizePriceService _priceService;

		public ShopPresentersFactory(
			IPurchaseService purchaseService,
			ITicketService ticketService,
			IAdvService advService,
			IRemoveAdsService removeAdsService,
			IResourcesService resourcesService,
			ILocalizePriceService priceService,
			IQuestMessageSender questMessageSender 
			)
		{
			_purchaseService = purchaseService;
			_ticketService = ticketService;
			_advService = advService;
			_removeAdsService = removeAdsService;
			_resourcesService = resourcesService;
			_priceService = priceService;
			_questMessageSender = questMessageSender;
		}

		public TicketPurchasePresenter CreateTicketPresenter(
			ShopButtonWidget shopButtonWidget,
			TicketModel model)
		{
			var ticket = new TicketPurchasePresenter(_purchaseService, shopButtonWidget, model, _priceService);
			ticket.Initialize();
			return ticket;
		}

		public ItemPurchasePresenter CreateItemPurchasePresenter<T>(ShopButtonWidget shopButtonWidget, ShopObjectItemModel<T> model) where T : ObjectData
		{
			var item = new ItemPurchasePresenter(_purchaseService, shopButtonWidget, model, model.Item.ObjectData, _priceService);
			item.Initialize();
			return item;
		}

		public FreeRewardTicketElementPresenter CreateFreeTicketPresenter(
			ShopButtonWidget shopButtonWidget)
		{
			return new FreeRewardTicketElementPresenter(shopButtonWidget, _advService, _resourcesService, _questMessageSender);
		}

		public NoAdsWidgetPresenter<T> CreateNoAdsPresenter<T>(ShopButtonWidget noAdsWidget, T removeAdsModel) where T : ShopItemModel, IRemoveAdsModel
		{
			var noads = new NoAdsWidgetPresenter<T>(_purchaseService, noAdsWidget, removeAdsModel, _removeAdsService,_priceService);
			noads.Initialize();
			return noads;
		} 
		
		public BundlePurchasePresenter CreateBundlePresenter(BundleButtonWidget widget, BundleShopItemModel bundleModel)
		{
			var bundle = new BundlePurchasePresenter(_purchaseService, widget, bundleModel,  _priceService);
			bundle.Initialize();
			return bundle;
		}
	}
}