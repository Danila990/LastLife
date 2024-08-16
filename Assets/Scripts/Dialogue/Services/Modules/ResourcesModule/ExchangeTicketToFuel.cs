using System;
using Core.Quests.Messages;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using TMPro;
using Ui.Widget;

namespace Dialogue.Services.Modules.ResourcesModule
{
	public class ExchangeTicketToFuel : AbstractBankMenuPresenter
	{
		private readonly Action _complete;

		public ExchangeTicketToFuel(IResourcesService resourcesService,
			BankMenuPanel bankMenuPanel,
			IBankService bankService,
			Action complete) 
			: base(resourcesService, bankMenuPanel, bankService)
		{
			_complete = complete;

		}
		
		protected override void CreateWidgets(out ResourceWidget leftWidget, out AddRemoveWidget addRemoveWidget, out TextMeshProUGUI simpleText, out NamedButtonWidget namedButtonWidget, out ResourceWidget rightWidget)
		{
			LeftCount = ResourcesService.GetCurrentResourceCount(ResourceType.Ticket);
			RightCount = 0;
			TransactionAmount = 0;
			
			BankMenuPanel.PlayerNameTXT.text = "You";
			BankMenuPanel.ArrowImg.sprite = BankMenuPanel.BothArrow;

			leftWidget = BankMenuPanel.CreateResourceWidget();
			leftWidget.SetResource(ResourceType.Ticket);
			
			addRemoveWidget = BankMenuPanel.CreateAddRemoveWidget();

			simpleText = BankMenuPanel.CreateSimpleText();
			
			namedButtonWidget = BankMenuPanel.CreateSimpleButtonWidget();
			namedButtonWidget.Button.onClick.AddListener(Submit);
			namedButtonWidget.Text.text = "Buy";
			
			rightWidget = BankMenuPanel.CreateResourceWidget();
			rightWidget.SetResource(ResourceType.Fuel);
			rightWidget.ReLayout(ResourceWidget.CountTextPosition.Left);
			
			BankMenuPanel.gameObject.SetActive(true);
			RefreshUi();
		}
		
		public override void Submit()
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, ResourceItemIds.REPAIRMAN_ITEM_ID);
			if (ResourcesService.TrySpendResource(ResourceType.Ticket, TransactionAmount, meta))
			{
				ResourcesService.AddResource(ResourceType.Fuel, TransactionAmount, meta);
				_complete();
			}
		}
	
		public override bool IncreaseAvailable() => LeftCount >= 1;
		public override bool DecreaseAvailable() => TransactionAmount > 0;
		public override void Increase()
		{
			LeftCount--;
			RightCount++;
			TransactionAmount++;
		}
		
		public override void Decrease()
		{
			LeftCount++;
			RightCount--;
			TransactionAmount--;
		}
	}

}