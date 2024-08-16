using System;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using TMPro;
using Ui.Widget;

namespace Dialogue.Services.Modules.ResourcesModule
{

	public class ExchangeSellBankMenuPresenter : AbstractBankMenuPresenter
	{
		private readonly Action _complete;
		public ExchangeSellBankMenuPresenter(IResourcesService resourcesService,
			BankMenuPanel bankMenuPanel,
			IBankService bankService,
			Action complete) 
			: base(resourcesService, bankMenuPanel, bankService)
		{
			_complete = complete;

		}
		
		public override void Submit()
		{
			if (BankService.TrySellGoldTickets(TransactionAmount))
			{
				_complete();
			}
		}

		protected override void CreateWidgets(
			out ResourceWidget leftWidget,
			out AddRemoveWidget addRemoveWidget, 
			out TextMeshProUGUI simpleText, 
			out NamedButtonWidget namedButtonWidget,
			out ResourceWidget rightWidget)
		{
			LeftCount = ResourcesService.GetCurrentResourceCount(ResourceType.GoldTicket);
			RightCount = 0;
			TransactionAmount = 0;
			
			BankMenuPanel.PlayerNameTXT.text = "You";
			BankMenuPanel.ArrowImg.sprite = BankMenuPanel.BothArrow;

			leftWidget = BankMenuPanel.CreateResourceWidget();
			leftWidget.SetResource(ResourceType.GoldTicket);
			
			addRemoveWidget = BankMenuPanel.CreateAddRemoveWidget();

			simpleText = BankMenuPanel.CreateSimpleText();
			
			namedButtonWidget = BankMenuPanel.CreateSimpleButtonWidget();
			namedButtonWidget.Button.onClick.AddListener(Submit);
			namedButtonWidget.Text.text = "Sell";
			namedButtonWidget.Button.image.sprite = BankMenuPanel.OrangeButtonSprite;
			
			rightWidget = BankMenuPanel.CreateResourceWidget();
			rightWidget.SetResource(ResourceType.Ticket);
			rightWidget.ReLayout(ResourceWidget.CountTextPosition.Left);
			
			BankMenuPanel.gameObject.SetActive(true);
		}
		
		public override bool IncreaseAvailable() => LeftCount > 0;
		public override bool DecreaseAvailable() => TransactionAmount > 0;
		
		public override void Increase()
		{
			LeftCount--;
			RightCount += 10;
			TransactionAmount++;
		}
		
		public override void Decrease()
		{
			LeftCount++;
			RightCount -= 10;
			TransactionAmount--;
		}
	}
}