using System;
using System.Collections;
using AnnulusGames.LucidTools.Audio;
using Core.Quests.Messages;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Dialogue.Services.Interfaces;
using Dialogue.Ui.CustomViews;
using Market.Bank;
using UniRx;
using UnityEngine;
using Yarn.Unity;

namespace Dialogue.Services.Modules.ResourcesModule
{
	public class ResourcesDialogueModule : IDialogueModule, IDisposable
	{
		private readonly IResourcesService _resourceService;
		private readonly BankMenuPanel _bankMenuPanel;
		public string ModuleId => nameof(ResourcesDialogueModule);

		private static ResourcesDialogueModule _instance;
		
		private readonly DepositBankMenuPresenter _depositPresenter;
		private readonly WithdrawBankMenuPresenter _withDrawPresenter;
		private readonly ExchangeSellBankMenuPresenter _exchangeSellPresenter;
		private readonly ExchangeBoughtBankMenuPresenter _exchangeBoughtPresenter;
		private readonly ExchangeTicketToFuel _exchangeTicketToFuel;
		private readonly ExchangeFuelToTicket _exchangeFuelToTicket;
		private readonly IDisposable _disposable;
		private readonly WaitUntil _waitUntil;
		public static AbstractBankMenuPresenter CurrentPresenter;

		public bool IsComplete { get; private set; } = false;

		public ResourcesDialogueModule(IResourcesService resourceService, IBankService bankService, BankMenuPanel bankMenuPanel, IQuestMessageSender questMessageSender)
		{
			_resourceService = resourceService;
			_bankMenuPanel = bankMenuPanel;
			_instance = this;

			_depositPresenter = new DepositBankMenuPresenter(resourceService, bankMenuPanel, bankService, SetComplete);
			_withDrawPresenter = new WithdrawBankMenuPresenter(resourceService, bankMenuPanel, bankService, SetComplete);
			
			_exchangeSellPresenter = new ExchangeSellBankMenuPresenter(resourceService, bankMenuPanel, bankService, SetComplete);
			_exchangeBoughtPresenter = new ExchangeBoughtBankMenuPresenter(resourceService, bankMenuPanel, bankService, SetComplete);
			
			_exchangeTicketToFuel = new ExchangeTicketToFuel(resourceService, bankMenuPanel, bankService, SetComplete);
			_exchangeFuelToTicket = new ExchangeFuelToTicket(resourceService, bankMenuPanel, bankService, questMessageSender, SetComplete);

			_disposable = bankMenuPanel.BackButton.OnClickAsObservable().Subscribe(_ => IsComplete = true);
			_waitUntil = new WaitUntil(Complete);
		}
		
		public void SetComplete()
		{
			LucidAudio.PlaySE(_bankMenuPanel.DealSound)
				.SetVolume(0.5f)
				.SetSpatialBlend(0f);
			IsComplete = true;
		}
		private bool Complete() => IsComplete;

		public void OnStartDialogue(IModuleArgs moduleArgs = null) { }
		
		public void OnDialogueEnd() { }
		
		public void AddCommand(DialogueRunner dialogueRunner) { }
		
		[YarnFunction("goldTicketCount")]
		public static int GoldTicketCount() 
			=> _instance._resourceService.GetCurrentResourceCount(ResourceType.GoldTicket);

		[YarnFunction("ticketCount")]
		public static int TicketCount() 
			=> _instance._resourceService.GetCurrentResourceCount(ResourceType.Ticket);

		[YarnFunction("goldTicketInBankCount")]
		public static int GoldTicketInBankCount() 
			=> _instance._resourceService.GetCurrentResourceCount(ResourceType.GoldTicketInBank);

		[YarnCommand("goldenToCommon")]
		public static IEnumerator SpendGoldenTickets(string msg, string merchantName) 
			=> ShowExchangeView(_instance._exchangeSellPresenter, msg, merchantName);

		[YarnCommand("commonToGold")]
		public static IEnumerator BoughtGoldenTickets(string msg, string merchantName) 
			=> ShowExchangeView(_instance._exchangeBoughtPresenter, msg, merchantName);

		[YarnCommand("ticketToFuel")]
		public static IEnumerator BoughtTickets(string msg, string merchantName) 
			=> ShowExchangeView(_instance._exchangeTicketToFuel, msg, merchantName);

		[YarnCommand("fuelToTicket")]
		public static IEnumerator BoughtFuel(string msg, string merchantName) 
			=> ShowExchangeView(_instance._exchangeFuelToTicket, msg, merchantName);

		[YarnCommand("depositGoldTickets")]
		public static IEnumerator DepositTickets(string msg, string merchantName) 
			=> ShowExchangeView(_instance._depositPresenter, msg, merchantName);

		[YarnCommand("withdrawGoldTickets")]
		public static IEnumerator WithdrawTickets(string msg, string merchantName) 
			=> ShowExchangeView(_instance._withDrawPresenter, msg, merchantName);

		private static IEnumerator ShowExchangeView(AbstractBankMenuPresenter presenter, string msg, string merchantName)
		{
			_instance.IsComplete = false;
			presenter.Show();
			_instance._bankMenuPanel.MessageTXT.text = msg;
			_instance._bankMenuPanel.RightTxt.text = merchantName;
			CurrentPresenter = presenter;
			yield return _instance._waitUntil;
			
			_instance._bankMenuPanel.gameObject.SetActive(false);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}