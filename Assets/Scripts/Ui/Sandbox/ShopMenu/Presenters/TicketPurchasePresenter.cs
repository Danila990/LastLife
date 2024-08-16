using Core.Equipment.Data;
using Db.ObjectData;
using Shop;
using Shop.Models;
using Shop.Models.Product;
using Ui.Widget;

namespace Ui.Sandbox.ShopMenu.Presenters
{
	public class TicketPurchasePresenter : PurchaseElementPresenter
	{
		public TicketPurchasePresenter(
			IPurchaseService purchaseService, 
			ShopButtonWidget shopButtonWidget, 
			TicketModel model,
			ILocalizePriceService priceService) : base(purchaseService, shopButtonWidget, model,priceService)
		{
			shopButtonWidget.IconImg.sprite = model.Ico;
			//shopButtonWidget.PriceTXT.text = model.Price;
			shopButtonWidget.ItemCountTXT.text = model.TicketCount.ToString();
			shopButtonWidget.ItemCountTXT.gameObject.SetActive(true);
		}
		
		protected override void SetPrice(InAppPrice price)
		{
			ShopButtonWidget.PriceTXT.text = price.ToString();
		}
	}

	public class ItemPurchasePresenter : PurchaseElementPresenter
	{
		public ObjectData ItemObjectData { get; }

		public ItemPurchasePresenter(IPurchaseService purchaseService,
			ShopButtonWidget shopButtonWidget,
			ShopItemModel model,
			ObjectData itemObjectData,
			ILocalizePriceService priceService)
			: base(purchaseService, shopButtonWidget, model,priceService)
		{
			ItemObjectData = itemObjectData;
			shopButtonWidget.IconImg.sprite = model.Ico;
			switch (itemObjectData)
			{
				//shopButtonWidget.PriceTXT.text = model.Price;
				case CharacterObjectData characterObjectData:
					shopButtonWidget.ItemAdditionalTXT.text = characterObjectData.Name + " " + characterObjectData.AdditionalName;
					shopButtonWidget.ItemAdditionalTXT.gameObject.SetActive(true);
					shopButtonWidget.NewHolder.SetActive(characterObjectData.IsNew);
					break;
				case EquipmentItemData equipmentItemData:
					shopButtonWidget.ItemAdditionalTXT.text = equipmentItemData.Name;
					shopButtonWidget.ItemAdditionalTXT.gameObject.SetActive(true);
					break;
				case BoostObjectData boostObjectData:
					shopButtonWidget.ItemAdditionalTXT.text = boostObjectData.Name;
					shopButtonWidget.ItemAdditionalTXT.gameObject.SetActive(true);
					break;
			}
		}

		protected override void SetPrice(InAppPrice price)
		{
			ShopButtonWidget.PriceTXT.text = price.ToString();
		}
		
		public void Highlight()
			=> ShopButtonWidget.Highlight();
	}
}