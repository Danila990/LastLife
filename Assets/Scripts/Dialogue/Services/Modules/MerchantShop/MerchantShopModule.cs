using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Actions.SpecialAbilities;
using Core.InputSystem;
using Core.ResourcesSystem.Interfaces;
using Core.Services;
using Db.MerchantData;
using Dialogue.Services.Interfaces;
using Dialogue.Ui;
using Dialogue.Ui.MerchantUi;
using GameSettings;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using UniRx;
using UnityEngine;
using Yarn.Unity;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class MerchantShopModule : IDialogueModule, IDisposable
	{
		private readonly MerchantShopPanel _merchantShopPanel;
		private readonly DialogueUiView _dialogueUiView;
		private readonly IDisposable _disposable;
		private readonly Dictionary<string, MerchantItemCollectionData> _merchantItemCollections;
		private readonly MerchantShopPresenter _merchantShopPresenter;
		private readonly ModifiersMerchantShopPresenter _modifiersPresenter;

		public static MerchantShopModule Instance;

		public string ModuleId => nameof(MerchantShopModule);
		private readonly WaitUntil _waitUntil;
		public bool IsComplete = false;
		private ShopDialogueModuleArgs _shopDialogueModuleArgs;
		private bool _status;
		
		public MerchantShopPresenter MerchantShopPresenter => _merchantShopPresenter;
		public ModifiersMerchantShopPresenter ModifiersMerchantShopPresenter => _modifiersPresenter;
		
		public MerchantShopModule(
			MerchantShopPanel merchantShopPanel, 
			IReadOnlyList<MerchantItemCollectionData> merchantItemCollection,
			IResourcesService resourceService,
			IPublisher<MessageObjectLocalPurchase> messagePublisher,
			ISettingsService settingsService,
			IItemStorage itemStorage,
			IItemUnlockService itemUnlockService,
			IPlayerSpawnService playerSpawnService,
			IAbilitiesControllerService abilitiesControllerService,
			DialogueUiView dialogueUiView
		)
		{
			_merchantShopPanel = merchantShopPanel;
			_dialogueUiView = dialogueUiView;
			_merchantItemCollections = merchantItemCollection.ToDictionary(x => x.Id);
			Instance = this;
			
			_merchantShopPresenter = new MerchantShopPresenter(
				_merchantShopPanel, 
				() => Complete(true),
				resourceService, 
				messagePublisher,
				itemStorage,
				itemUnlockService
				);
			
			_modifiersPresenter = new ModifiersMerchantShopPresenter(
				_merchantShopPanel, 
				() => Complete(true),
				resourceService,
				messagePublisher,
				settingsService,
				itemStorage,
				itemUnlockService,
				playerSpawnService, 
				abilitiesControllerService
				);
			
			_disposable = _merchantShopPanel.BackButton.OnClickAsObservable().Subscribe(_ => Complete(false));
			_waitUntil = new WaitUntil(() => IsComplete);
		}

		public void Complete(bool status)
		{
			_status = status;
			IsComplete = true;
			_merchantShopPanel.gameObject.SetActive(false);
		}

		public void OnStartDialogue(IModuleArgs moduleArgs = null)
		{
			if (moduleArgs is ShopDialogueModuleArgs shopDialogueModuleArgs)
			{
				_shopDialogueModuleArgs = shopDialogueModuleArgs;
			}
		}
		
		public void OnDialogueEnd()
		{
			_shopDialogueModuleArgs = null;
		}
		
		public void AddCommand(DialogueRunner dialogueRunner) { }
		
		
		[YarnCommand("merchantShop")]
		public static IEnumerator BuyItems(string msg, string itemsCollectionsIds, string merchantName)
		{
			Instance.IsComplete = false;
			
			if (Instance._merchantItemCollections.TryGetValue(itemsCollectionsIds, out var collection))
				Instance.OpenPresenter(Instance._merchantShopPresenter, collection, merchantName, msg);
			
			yield return Instance._waitUntil;
		}
		
		[YarnCommand("modifiersShop")]
		public static IEnumerator BuyModifiersItems(string msg, string itemsCollectionsIds, string merchantName, string buttonTxt)
		{
			Instance.IsComplete = false;

			if (Instance._merchantItemCollections.TryGetValue(itemsCollectionsIds, out var collection))
			{
				Instance.OpenPresenter(Instance._modifiersPresenter, collection, merchantName, msg);
				Instance._merchantShopPanel.SubmitButton.Text.text = buttonTxt;
				Instance._merchantShopPanel.SubmitButton.SetWidthByText(30);
			}
			
			yield return Instance._waitUntil;
		}
		
		private void OpenPresenter(
			AbstractMerchantShopPresenter instanceMerchantShopPresenter,
			IMerchantItemCollectionData collection,
			string merchantName, 
			string msg)
		{
			instanceMerchantShopPresenter.OpenFor(collection, merchantName, msg, Instance._shopDialogueModuleArgs);
		}

		[YarnFunction("boughtItems")]
		public static bool BoughtItems()
		{
			var status  = Instance._status;
			Instance._status = false;
			return status;
		}
		
		[YarnCommand("hideControl")]
		public static void HideControl()
		{
			Instance._dialogueUiView.CanvasGroup.interactable = false;
			Instance._dialogueUiView.CanvasGroup.alpha = 0;
		}
		
		[YarnCommand("enableControl")]
		public static void EnableControl()
		{
			Instance._dialogueUiView.CanvasGroup.interactable = true;
			Instance._dialogueUiView.CanvasGroup.alpha = 1;
		}
		
		public void Dispose()
		{
			Instance = null;
			_disposable?.Dispose();
			_merchantShopPresenter?.Dispose();
		}
	}
}