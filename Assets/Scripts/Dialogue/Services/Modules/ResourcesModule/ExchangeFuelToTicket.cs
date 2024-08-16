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
	public class ExchangeFuelToTicket : AbstractBankMenuPresenter
	{
		private readonly IQuestMessageSender _questMessageSender;
		private readonly Action _complete;

		public ExchangeFuelToTicket(IResourcesService resourcesService,
			BankMenuPanel bankMenuPanel,
			IBankService bankService,
			IQuestMessageSender questMessageSender,
			Action complete)
			: base(resourcesService, bankMenuPanel, bankService)
		{
			_questMessageSender = questMessageSender;
			_complete = complete;
		}
		protected override void CreateWidgets(out ResourceWidget leftWidget, out AddRemoveWidget addRemoveWidget, out TextMeshProUGUI simpleText, out NamedButtonWidget namedButtonWidget, out ResourceWidget rightWidget)
		{
			LeftCount = ResourcesService.GetCurrentResourceCount(ResourceType.Fuel);
			RightCount = 0;
			TransactionAmount = 0;
			
			BankMenuPanel.PlayerNameTXT.text = "You";
			BankMenuPanel.ArrowImg.sprite = BankMenuPanel.BothArrow;

			leftWidget = BankMenuPanel.CreateResourceWidget();
			leftWidget.SetResource(ResourceType.Fuel);
			
			addRemoveWidget = BankMenuPanel.CreateAddRemoveWidget();

			simpleText = BankMenuPanel.CreateSimpleText();
			
			namedButtonWidget = BankMenuPanel.CreateSimpleButtonWidget();
			namedButtonWidget.Button.onClick.AddListener(Submit);
			namedButtonWidget.Text.text = "Sell";
			
			rightWidget = BankMenuPanel.CreateResourceWidget();
			rightWidget.SetResource(ResourceType.Ticket);
			rightWidget.ReLayout(ResourceWidget.CountTextPosition.Left);
			
			BankMenuPanel.gameObject.SetActive(true);
			RefreshUi();
		}
		
		public override void Submit()
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, ResourceItemIds.REPAIRMAN_ITEM_ID);
			if (ResourcesService.TrySpendResource(ResourceType.Fuel, TransactionAmount, meta))
			{
				ResourcesService.AddResource(ResourceType.Ticket, TransactionAmount, meta);
				_complete();
				_questMessageSender.SendExchangeMessage(ResourceType.Fuel.ToString(), ResourceType.Ticket.ToString());
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
