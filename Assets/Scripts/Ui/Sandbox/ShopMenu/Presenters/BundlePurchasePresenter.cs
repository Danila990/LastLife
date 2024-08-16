using System.Globalization;
using Shop;
using Shop.Models;
using Shop.Models.Product;
using Ui.Widget;
using UnityEngine;

namespace Ui.Sandbox.ShopMenu.Presenters
{
	public class BundlePurchasePresenter : PurchaseElementPresenter
	{
		private readonly BundleButtonWidget _shopButtonWidget;
		new public BundleShopItemModel Model { get; }
		public BundlePurchasePresenter(
			IPurchaseService purchaseService,
			BundleButtonWidget shopButtonWidget, 
			BundleShopItemModel model, 
			ILocalizePriceService localizePriceService)
			:
			base(
				purchaseService, 
				shopButtonWidget, 
				model, 
				localizePriceService)

		{
			Model = model;
			_shopButtonWidget = shopButtonWidget;
			shopButtonWidget.IconImg.sprite = model.Ico;
		}

		protected override void SetPrice(InAppPrice price)
		{
			_shopButtonWidget.PriceTXT.text = price.ToString();
			_shopButtonWidget.OldPrice.text = $"{price.ISO} {(price.Price * Model.DiscountMlp).ToString("F2", CultureInfo.InvariantCulture)}";
			_shopButtonWidget.OldPrice.rectTransform.sizeDelta = new Vector2(_shopButtonWidget.OldPrice.GetPreferredValues().x + 10, _shopButtonWidget.OldPrice.rectTransform.sizeDelta.y);
		}
	}
}