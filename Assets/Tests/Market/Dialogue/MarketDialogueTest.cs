using System.Collections;
using System.Linq;
using Core.Entity.Ai.Merchant;
using Core.InputSystem;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Cysharp.Threading.Tasks;
using Dialogue.Services;
using Dialogue.Services.Modules.MerchantShop;
using Dialogue.Services.Modules.ResourcesModule;
using Dialogue.Ui;
using Dialogue.Ui.CustomViews;
using Dialogue.Ui.MerchantUi;
using MessagePipe;
using NUnit.Framework;
using Tests.UITest;
using Ticket;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainerUi.Messages;

namespace Tests.Market.Dialogue
{
	public class MarketDialogueTest : UiTest
	{
		[UnityTest]
		public IEnumerator TestDialogues() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitMarket(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var dialogueUiController = scope.Container.Resolve<DialogueUiView>();
			var dialogueService = scope.Container.Resolve<IDialogueService>() as DialogueService;
			await UniTask.Delay(1000, cancellationToken: CancellationToken);

			var allNpc = Object.FindObjectsByType<DialogueMonoInteraction>(FindObjectsSortMode.None);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);

			foreach (var npc in allNpc)
			{
				npc.Use(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
				Debug.Log("Use dialoge");
				await UniTask.WaitUntil((() => dialogueService.DialogueRunner.IsDialogueRunning), cancellationToken: CancellationToken);
				Debug.Log("Start dialoge");
				var dialogeView = Object.FindFirstObjectByType<CustomLineView>();
				for (var i = 0; i < 15; i++)
				{
					await Press(dialogeView.continueButton);
					await UniTask.Delay(100, cancellationToken: CancellationToken);
				}
				await Press(dialogueUiController.StopDialogue.gameObject);
				await UniTask.Delay(100, cancellationToken: CancellationToken);
			}
		});

		[UnityTest]
		public IEnumerator TestShop() => UniTask.ToCoroutine(async () =>
		{
			PlayerPrefs.DeleteAll();
			var scope = await InitMarket(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var dialogueUiController = scope.Container.Resolve<DialogueUiView>();
			var publisher = scope.Container.Resolve<IPublisher<MessageOpenWindow>>();
			scope.Container.Resolve<ITicketService>().OnPurchaseTickets(200);
			scope.Container.Resolve<IResourcesService>().AddResource(ResourceType.GoldTicket, 200, new ResourceEventMetaData());
			var dialogueService = scope.Container.Resolve<IDialogueService>() as DialogueService;
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			//var allNpc = Object.FindObjectsByType<DialogueMonoInteraction>(FindObjectsSortMode.None);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			publisher.OpenWindow<DialogueWindow>();

			UniTask.Void(async () => await ResourcesDialogueModule.BoughtFuel("213", "123"));
			await TestShopPanel();
			UniTask.Void(async () => await ResourcesDialogueModule.BoughtTickets("213", "123"));
			await TestShopPanel();
			UniTask.Void(async () => await ResourcesDialogueModule.DepositTickets("213", "123"));
			await TestShopPanel();
			UniTask.Void(async () => await ResourcesDialogueModule.WithdrawTickets("213", "123"));
			await TestShopPanel();
			UniTask.Void(async () => await ResourcesDialogueModule.BoughtGoldenTickets("213", "123"));
			await TestShopPanel();
			UniTask.Void(async () => await ResourcesDialogueModule.SpendGoldenTickets("213", "123"));
			await TestShopPanel();
			var shopPanel = scope.Container.Resolve<MerchantShopPanel>();
			UniTask.Void(async () => await MerchantShopModule.BuyItems("Boosts","Girl", "Sugar Babe" ));
			foreach (var presenter in MerchantShopModule.Instance.MerchantShopPresenter.CurrentCardPresenters)
			{
				for (int i = 0; i < 10; i++)
				{
					await Press(presenter.Widget.AddRemoveWidget.AddBTN.gameObject);
					await UniTask.NextFrame(CancellationToken);
				}
				for (int i = 0; i < 5; i++)
				{
					await Press(presenter.Widget.AddRemoveWidget.RemoveBTN.gameObject);
					await UniTask.NextFrame(CancellationToken);
				}
			}
			MerchantShopModule.Instance.MerchantShopPresenter.OnClickSubmit(default);
			Assert.True(MerchantShopModule.BoughtItems());
			Assert.False(MerchantShopModule.BoughtItems());
			UniTask.Void(async () => await MerchantShopModule.BuyModifiersItems("Abilities", "ModifiersItemCollection", "Item Merchant", "Test"));

			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			await Press(shopPanel.SubmitButton.gameObject);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		private async UniTask TestShopPanel()
		{
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var i = 0;
			
			while (ResourcesDialogueModule.CurrentPresenter.IncreaseAvailable() && i < 15)
			{
				ResourcesDialogueModule.CurrentPresenter.Increase();
				ResourcesDialogueModule.CurrentPresenter.RefreshUi();
				i++;
				await UniTask.NextFrame(CancellationToken);
			}
			var decreaseCount = i / 2;
			
			for (int j = 0; j < decreaseCount && ResourcesDialogueModule.CurrentPresenter.DecreaseAvailable(); j++)
			{
				ResourcesDialogueModule.CurrentPresenter.Decrease();
				ResourcesDialogueModule.CurrentPresenter.RefreshUi();
				await UniTask.NextFrame(CancellationToken);
			}
			
			ResourcesDialogueModule.CurrentPresenter.RefreshUi();
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			ResourcesDialogueModule.CurrentPresenter.Submit();
		}
	}

}