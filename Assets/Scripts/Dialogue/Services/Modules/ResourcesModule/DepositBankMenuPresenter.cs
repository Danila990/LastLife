using System;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using TMPro;
using Ui.Widget;
using UnityEngine;

namespace Dialogue.Services.Modules.ResourcesModule
{
	public class DepositBankMenuPresenter : AbstractBankMenuPresenter
	{
		private readonly Action _complete;
		public DepositBankMenuPresenter(IResourcesService resourcesService,
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
			BankMenuPanel.ArrowImg.transform.localScale = new Vector3(-1,1,1);

			leftWidget = BankMenuPanel.CreateResourceWidget();
			leftWidget.SetResource(ResourceType.GoldTicket);
			
			addRemoveWidget = BankMenuPanel.CreateAddRemoveWidget();

			simpleText = BankMenuPanel.CreateSimpleText();
			
			namedButtonWidget = BankMenuPanel.CreateSimpleButtonWidget();
			namedButtonWidget.Button.onClick.AddListener(Submit);
			namedButtonWidget.Text.text = "DEPOSIT";
			
			rightWidget = BankMenuPanel.CreateResourceWidget();
			rightWidget.SetResource(ResourceType.GoldTicket);
			rightWidget.ReLayout(ResourceWidget.CountTextPosition.Left);
			
			BankMenuPanel.gameObject.SetActive(true);
		}
		
		public override void Submit()
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, ResourceItemIds.BANKER_ITEM_ID);
			if (ResourcesService.TrySpendResource(ResourceType.GoldTicket, TransactionAmount, meta))
			{
				ResourcesService.AddResource(ResourceType.GoldTicketInBank, TransactionAmount, meta);
				_complete();
			}
		}
	
		public override bool IncreaseAvailable() => LeftCount > 0;
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