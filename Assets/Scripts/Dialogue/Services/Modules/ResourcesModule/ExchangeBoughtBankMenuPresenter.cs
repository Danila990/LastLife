using System;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using TMPro;
using Ui.Widget;

namespace Dialogue.Services.Modules.ResourcesModule
{
	public class ExchangeBoughtBankMenuPresenter : AbstractBankMenuPresenter
	{
		private readonly Action _complete;
		public ExchangeBoughtBankMenuPresenter(IResourcesService resourcesService,
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
			rightWidget.SetResource(ResourceType.GoldTicket);
			rightWidget.ReLayout(ResourceWidget.CountTextPosition.Left);
			
			BankMenuPanel.gameObject.SetActive(true);
			RefreshUi();
		}
		
		public override void Submit()
		{
			if (BankService.TryBuyGoldTickets(TransactionAmount))
			{
				_complete();
			}
		}
		
		public override bool IncreaseAvailable() => LeftCount >= 10;
		public override bool DecreaseAvailable() => TransactionAmount > 0;
		
		public override void Increase()
		{
			LeftCount -= 10;
			RightCount++;
			TransactionAmount++;
		}
		
		public override void Decrease()
		{
			LeftCount += 10;
			RightCount--;
			TransactionAmount--;
		}
	}
}