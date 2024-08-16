using Core.ResourcesSystem.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using TMPro;
using Ui.Widget;

namespace Dialogue.Services.Modules.ResourcesModule
{
	public abstract class AbstractBankMenuPresenter
	{
		protected readonly IResourcesService ResourcesService;
		protected readonly BankMenuPanel BankMenuPanel;
		protected readonly IBankService BankService;
		
		
		protected ResourceWidget LeftResourceWidget;
		protected AddRemoveWidget AddRemove;
		protected TextMeshProUGUI SimpleText;
		protected NamedButtonWidget SubmitButton;
		protected ResourceWidget RightResourceWidget;
		
		public int LeftCount;
		public int RightCount;
		public int TransactionAmount;
		
		protected AbstractBankMenuPresenter(
			IResourcesService resourcesService, 
			BankMenuPanel bankMenuPanel,
			IBankService bankService)
		{
			ResourcesService = resourcesService;
			BankMenuPanel = bankMenuPanel;
			BankService = bankService;
		}

		public void Show()
		{
			BankMenuPanel.Clear();
			
			CreateWidgets(
				out LeftResourceWidget,
				out AddRemove,
				out SimpleText,
				out SubmitButton,
				out RightResourceWidget);
			
			AddRemove.AddOrRemoveEvent += OnAddRemove;
			BankMenuPanel.Relayout();
			RefreshUi();
		}

		protected abstract void CreateWidgets(
			out ResourceWidget leftWidget,
			out AddRemoveWidget addRemoveWidget,
			out TextMeshProUGUI simpleText,
			out NamedButtonWidget namedButtonWidget,
			out ResourceWidget rightWidget);

		public void RefreshUi()
		{
			LeftResourceWidget.SetCount(LeftCount);
			RightResourceWidget.SetCount(RightCount);
			
			SimpleText.text = TransactionAmount.ToString();
			
			SubmitButton.Button.interactable = TransactionAmount > 0;
			
			AddRemove.AddBTN.interactable = IncreaseAvailable();
			AddRemove.RemoveBTN.interactable = DecreaseAvailable();
		}
		
		public virtual void Submit()
		{
		}
		
		protected void OnAddRemove(bool isAdd)
		{
			if (isAdd)
			{
				if (IncreaseAvailable())
				{
					Increase();
				}
			}
			else
			{
				if (DecreaseAvailable())
				{
					Decrease();
				}
			}
			RefreshUi();
		}

		public abstract bool IncreaseAvailable();
		public abstract bool DecreaseAvailable();
		
		public abstract void Increase();
		public abstract void Decrease();
	}
}