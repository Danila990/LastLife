using System;
using System.Collections.Generic;
using System.Linq;
using AnnulusGames.LucidTools.Audio;
using Core.Actions.SpecialAbilities;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Core.Services;
using Db.MerchantData;
using Dialogue.Services.Interfaces;
using Dialogue.Ui.MerchantUi;
using GameSettings;
using MessagePipe;
using UniRx;
using UnityEngine;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class ModifiersMerchantShopPresenter : AbstractMerchantShopPresenter
	{
		private readonly MerchantShopPanel _merchantShopPanel;
		private readonly IResourcesService _resourceService;

		private readonly Action _onComplete;
		private readonly IPublisher<MessageObjectLocalPurchase> _messagePublisher;
		
		private readonly ISettingsService _settingsService;
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IAbilitiesControllerService _abilitiesControllerService;
		private readonly List<ModifierMerchantCardPresenter> _cardPresenters = new List<ModifierMerchantCardPresenter>();
		private readonly IntReactiveProperty _currentResourceCount = new IntReactiveProperty();

		private ResourceType _collectionResourceTypeCount;
		private int _initialCount;
		private CompositeDisposable _runtimeDisposable;
		private ShopDialogueModuleArgs _shopDialogueModuleArgs;
		public ModifierMerchantCardPresenter CurrentSelectedModifier { get; set; }


		public ModifiersMerchantShopPresenter(
			MerchantShopPanel merchantShopPanel, 
			Action action, 
			IResourcesService resourceService, 
			IPublisher<MessageObjectLocalPurchase> messagePublisher,
			ISettingsService settingsService, 
			IItemStorage itemStorage,
			IItemUnlockService itemUnlockService,
			IPlayerSpawnService playerSpawnService,
			IAbilitiesControllerService abilitiesControllerService)
		{
			_merchantShopPanel = merchantShopPanel;
			_onComplete = action;
			_resourceService = resourceService;
			_messagePublisher = messagePublisher;
			_settingsService = settingsService;
			_itemStorage = itemStorage;
			_itemUnlockService = itemUnlockService;
			_playerSpawnService = playerSpawnService;
			_abilitiesControllerService = abilitiesControllerService;
		}

		public override void OpenFor(IMerchantItemCollectionData collection, string merchantName, string msg, ShopDialogueModuleArgs instanceShopDialogueModuleArgs)
		{
			_shopDialogueModuleArgs = null;
			if (instanceShopDialogueModuleArgs != null)
				_shopDialogueModuleArgs = instanceShopDialogueModuleArgs;
			
			_runtimeDisposable?.Dispose();
			_runtimeDisposable = new CompositeDisposable();
			CurrentSelectedModifier = null;
			
			_merchantShopPanel.Clear();
			_merchantShopPanel.SubmitButton.Button.OnClickAsObservable().Subscribe(OnClickSubmit).AddTo(_runtimeDisposable);
			_merchantShopPanel.gameObject.SetActive(true);

			_cardPresenters.Clear();
	
			
			_merchantShopPanel.GridLayout.cellSize = new Vector2(_merchantShopPanel.GridLayout.cellSize.x, 250);
			_merchantShopPanel.Rt.sizeDelta = new Vector2(_merchantShopPanel.Rt.sizeDelta.x, 740);
			_merchantShopPanel.MessageTXT.text = msg;
			_merchantShopPanel.RightText.text = merchantName;
			
			_collectionResourceTypeCount = collection.Items.First().Model.ResourceType;
			_merchantShopPanel.FullPrice.SetResource(_collectionResourceTypeCount);

			
			_initialCount = _resourceService.GetCurrentResourceCount(_collectionResourceTypeCount);
			_resourceService.GetResourceObservable(_collectionResourceTypeCount).Subscribe(OnResourceUpdate).AddTo(_runtimeDisposable);

			_currentResourceCount.Value = _initialCount;
			
			_merchantShopPanel.CurrentResourceCount.gameObject.SetActive(false);
			
			
			foreach (var item in collection.Items)
			{
				CreatePresenter(item);
			}
			CheckDuplicateActions();
			_merchantShopPanel.ReLayout();

			OnSelectedCardPresenter(_cardPresenters.FirstOrDefault(x => x.IsActive));
		}
		
		private void OnResourceUpdate(int resourceCount)
		{
			var total = CurrentSelectedModifier?.Price ?? 0;
			_initialCount = resourceCount;
			_currentResourceCount.Value = total;

			if (CurrentSelectedModifier is null)
				return;
			OnSelectedCardPresenter(CurrentSelectedModifier);
		}

		private void CheckDuplicateActions()
		{
			var weapon = _playerSpawnService.PlayerCharacterAdapter.CurrentContext
				.Inventory
				.InventoryItems
				.First(pair => pair.ItemContext is ProjectileWeaponContext)
				.ItemContext;
			
			foreach (var presenter in _cardPresenters)
			{
				if (!presenter.IsAvailable())
				{
					presenter.Disable();
					continue;
				}
				if (!_abilitiesControllerService.AbilityControllers.TryGetValue(presenter.Data.ObjectDataIdToBuy, out var ab))
					continue;
				
				foreach (var action in weapon.ActionProvider.ActionControllers)
				{
					if (action.EntityAction.Id == ab.Data.TargetAction.Id)
					{
						presenter.Disable();
						break;
					}	
				}
			}
		}

		protected override void CreatePresenter(MerchantShopItemDataSo item)
		{
			var widget = _merchantShopPanel.ModifiersCardWidget();
			
			var presenter = new ModifierMerchantCardPresenter(
					item.Model,
					widget,
					_settingsService,
					_itemStorage,
					_itemUnlockService)
				.AddTo(_runtimeDisposable);

			
			
			presenter.Show(_currentResourceCount);
			presenter.Clicked?.Subscribe(OnSelectedCardPresenter).AddTo(_runtimeDisposable);
			_cardPresenters.Add(presenter);
		}
		
		private void OnSelectedCardPresenter(ModifierMerchantCardPresenter selected)
		{
			if (selected is null)
			{
				_currentResourceCount.Value = 0;
				_merchantShopPanel.SubmitButton.Button.interactable = false;
				_merchantShopPanel.CurrentResourceCount.SetCount(_currentResourceCount.Value);
				_merchantShopPanel.FullPrice.SetCount(0);
				return;
			}
			CurrentSelectedModifier?.Deselect();
			CurrentSelectedModifier = selected;
			CurrentSelectedModifier.Select();

			var totalPrice = CurrentSelectedModifier.Price;
			_currentResourceCount.Value = _initialCount - totalPrice;
		
			_merchantShopPanel.FullPrice.SetCount(totalPrice);
			_merchantShopPanel.SubmitButton.Button.interactable = 
				totalPrice > 0 && 
				_resourceService.GetCurrentResourceCount(_collectionResourceTypeCount) >= totalPrice;
			
			_merchantShopPanel.CurrentResourceCount.SetCount(_currentResourceCount.Value);
		}

		public override void OnClickSubmit(Unit obj)
		{
			var totalPrice = CurrentSelectedModifier.Price;

			if (_resourceService.TrySpendResource(
				    _collectionResourceTypeCount,
				    totalPrice, 
				    new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, CurrentSelectedModifier.Data.Id)))
			{
				var message = new MessageObjectLocalPurchase(
					CurrentSelectedModifier.Data.ObjectDataIdToBuy,
					1,
					_shopDialogueModuleArgs);
				
				if (!string.IsNullOrEmpty(CurrentSelectedModifier.Data.UnlockKey))
				{
					_itemUnlockService.UnlockItem(CurrentSelectedModifier.Data);
				}
				
				_messagePublisher.Publish(message);
				
				_runtimeDisposable?.Dispose();
				_onComplete();
				
				LucidAudio.PlaySE(_merchantShopPanel.DealSound)
					.SetVolume(0.5f)
					.SetSpatialBlend(0f);

				if (_shopDialogueModuleArgs)
					_shopDialogueModuleArgs.Blackboard.SetVariableValue("UseCustomAnimation", true);
				
				_shopDialogueModuleArgs = null;
			}
		}
		
		public override void Dispose()
		{
			_runtimeDisposable?.Dispose();
			_currentResourceCount?.Dispose();
		}
	}
}