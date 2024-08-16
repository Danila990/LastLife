using System;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using TMPro;
using Ui.Widget;

namespace Dialogue.Services.Modules.ResourcesModule
{

	public class WithdrawBankMenuPresenter : AbstractBankMenuPresenter
	{
		private readonly Action _complete;
		public WithdrawBankMenuPresenter(IResourcesService resourcesService,
			BankMenuPanel bankMenuPanel,
			IBankService bankService,
			Action complete) 
			: base(resourcesService, bankMenuPanel, bankService)
		{
			_complete = complete;

		}
		
		protected override void CreateWidgets(
			out ResourceWidget leftWidget,
			out AddRemoveWidget addRemoveWidget, 
			out TextMeshProUGUI simpleText, 
			out NamedButtonWidget namedButtonWidget,
			out ResourceWidget rightWidget)
		{
			LeftCount = ResourcesService.GetCurrentResourceCount(ResourceType.GoldTicket);
			RightCount = ResourcesService.GetCurrentResourceCount(ResourceType.GoldTicketInBank);
			TransactionAmount = 0;
			
			BankMenuPanel.PlayerNameTXT.text = "You";
			BankMenuPanel.ArrowImg.sprite = BankMenuPanel.LeftArrow;

			leftWidget = BankMenuPanel.CreateResourceWidget();
			leftWidget.SetResource(ResourceType.GoldTicket);
			
			namedButtonWidget = BankMenuPanel.CreateSimpleButtonWidget();
			namedButtonWidget.Button.onClick.AddListener(Submit);
			namedButtonWidget.Text.text = "WITHDRAW";
			namedButtonWidget.Button.image.sprite = BankMenuPanel.OrangeButtonSprite;


			simpleText = BankMenuPanel.CreateSimpleText();
			
			addRemoveWidget = BankMenuPanel.CreateAddRemoveWidget();
			
			rightWidget = BankMenuPanel.CreateResourceWidget();
			rightWidget.SetResource(ResourceType.GoldTicket);
			rightWidget.ReLayout(ResourceWidget.CountTextPosition.Left);
			
			BankMenuPanel.gameObject.SetActive(true);
			RefreshUi();
		}
		
		public override void Submit()
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, ResourceItemIds.BANKER_ITEM_ID);
			if (ResourcesService.TrySpendResource(ResourceType.GoldTicketInBank, TransactionAmount, meta))
			{
				ResourcesService.AddResource(ResourceType.GoldTicket, TransactionAmount, meta);
				_complete();
			}
		}

		public override bool IncreaseAvailable() => RightCount > 0;
		public override bool DecreaseAvailable() => TransactionAmount > 0;
		
		public override void Increase()
		{
			LeftCount++;
			RightCount--;
			TransactionAmount++;
		}
		
		public override void Decrease()
		{
			LeftCount--;
			RightCount++;
			TransactionAmount--;
		}
	}
}