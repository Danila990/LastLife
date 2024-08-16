using Adv.Services;
using Adv.Services.Interfaces;
using Shop;
using Shop.Models;
using Shop.Models.Product;
using Ui.Widget;

namespace Ui.Sandbox.ShopMenu.Presenters
{
	public class NoAdsWidgetPresenter<T> : PurchaseElementPresenter where T : ShopItemModel, IPurchasable, IRemoveAdsModel
	{

		public NoAdsWidgetPresenter(IPurchaseService purchaseService,
			ShopButtonWidget shopButtonWidget,
			T model,
			IRemoveAdsService removeAdsService,
			ILocalizePriceService priceService) : 
			base(purchaseService, shopButtonWidget, model,priceService)
		{
			if (removeAdsService.IsRemoveAdsEnabled)
			{
				shopButtonWidget.Button.interactable = false;
			}

			//shopButtonWidget.PriceTXT.text = model.Price;
		}
		
		protected override void SetPrice(InAppPrice price)
		{
			ShopButtonWidget.PriceTXT.text = price.ToString();
		}
	}
}