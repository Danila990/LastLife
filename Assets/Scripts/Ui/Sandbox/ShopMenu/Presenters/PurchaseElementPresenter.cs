using Shop;
using Shop.Models;
using Shop.Models.Product;
using Ui.Widget;
using UniRx;

namespace Ui.Sandbox.ShopMenu.Presenters
{

	public abstract class PurchaseElementPresenter
	{
		protected readonly ShopButtonWidget ShopButtonWidget;
		private readonly IPurchaseService _purchaseService;
		private readonly ShopItemModel _model;
		private readonly ILocalizePriceService _localizePriceService;

		public ShopItemModel Model => _model;
		
		public PurchaseElementPresenter(
			IPurchaseService purchaseService,
			ShopButtonWidget shopButtonWidget,
			ShopItemModel model,
			ILocalizePriceService localizePriceService)
		{
			_purchaseService = purchaseService;
			ShopButtonWidget = shopButtonWidget;
			_model = model;
			_localizePriceService = localizePriceService;
			ShopButtonWidget.Button.onClick.AddListener(Buy);
		}

		public void Initialize()
		{
			_localizePriceService.Prices[_model.InAppId].Subscribe(SetPrice).AddTo(ShopButtonWidget);
		}
		
		protected virtual void SetPrice(InAppPrice price)
		{
		}
		
		private void Buy()
		{
			_purchaseService.PurchaseItem(_model);
		}
		
		public void Disable()
		{
			ShopButtonWidget.Button.onClick.RemoveAllListeners();
			ShopButtonWidget.Button.interactable = false;
		}
	}
}