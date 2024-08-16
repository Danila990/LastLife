using System;
using System.Collections.Generic;
using System.Linq;
using AnnulusGames.LucidTools.Audio;
using Common.SpawnPoint;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Core.Services;
using Db.MerchantData;
using Dialogue.Services.Interfaces;
using Dialogue.Ui.MerchantUi;
using GameSettings;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using Ui.Widget;
using UniRx;
using UnityEngine;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class MerchantShopPresenter : AbstractMerchantShopPresenter
	{
		private readonly MerchantShopPanel _merchantShopPanel;
		private readonly Action _onComplete;
		private readonly IResourcesService _resourcesService;
		private readonly IPublisher<MessageObjectLocalPurchase> _localObjectPurchasePublisher;
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _unlockService;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly List<SimpleMerchantCardPresenter> _cardPresenters = new List<SimpleMerchantCardPresenter>();
		public IReadOnlyList<SimpleMerchantCardPresenter> CurrentCardPresenters => _cardPresenters;
		
		private readonly IntReactiveProperty _currentResourceCount = new IntReactiveProperty();
		private MerchantShopItemPointManager _merchantShopItemPointManager;

		private CompositeDisposable _runtimeDisposable;
		private ResourceType _collectionResourceTypeCount;
		private int _initialCount;

		public MerchantShopPresenter(
			MerchantShopPanel merchantShopPanel,
			Action onComplete,
			IResourcesService resourcesService, 
			IPublisher<MessageObjectLocalPurchase> localObjectPurchasePublisher,
			IItemStorage itemStorage,
			IItemUnlockService unlockService
		)
		{
			_merchantShopPanel = merchantShopPanel;
			_onComplete = onComplete;
			_resourcesService = resourcesService;
			_localObjectPurchasePublisher = localObjectPurchasePublisher;
			_itemStorage = itemStorage;
			_unlockService = unlockService;
		}

		public override void OpenFor(IMerchantItemCollectionData collection, string merchantName, string msg, ShopDialogueModuleArgs instanceShopDialogueModuleArgs)
		{
			_runtimeDisposable?.Dispose();
			_runtimeDisposable = new CompositeDisposable();	
			
			_merchantShopPanel.SubmitButton.Button.OnClickAsObservable().Subscribe(OnClickSubmit).AddTo(_runtimeDisposable);
			_merchantShopItemPointManager = null;
			if (instanceShopDialogueModuleArgs != null)
				_merchantShopItemPointManager = instanceShopDialogueModuleArgs.ShopItemPointManager;
			
			_merchantShopPanel.Clear();
			_cardPresenters.Clear();
			
			_merchantShopPanel.gameObject.SetActive(true);
			_merchantShopPanel.MessageTXT.text = msg;
			_merchantShopPanel.RightText.text = merchantName;
			
			_collectionResourceTypeCount = collection.Items.First().Model.ResourceType;
			_merchantShopPanel.FullPrice.SetResource(_collectionResourceTypeCount);
			
			_initialCount = _resourcesService.GetCurrentResourceCount(_collectionResourceTypeCount);
			_resourcesService.GetResourceObservable(_collectionResourceTypeCount).Subscribe(OnResourceUpdate).AddTo(_runtimeDisposable);
			_currentResourceCount.Value = _initialCount;
			
			_merchantShopPanel.CurrentResourceCount.SetResource(_collectionResourceTypeCount);
			_merchantShopPanel.CurrentResourceCount.SetCount(_currentResourceCount.Value);
			_merchantShopPanel.CurrentResourceCount.ReLayout(ResourceWidget.CountTextPosition.Right);

			foreach (var item in collection.Items)
			{
				CreatePresenter(item);
			}
			_merchantShopPanel.ReLayout();
		}
		
		private void OnResourceUpdate(int resourceCount)
		{
			_initialCount = resourceCount;
			_currentResourceCount.Value = _initialCount - _cardPresenters.Sum(x => x.GetPriceByCount());
			OnCartCountChanged(0);
		}

		private void OnCartCountChanged(int countInCart)
		{
			var totalPrice = _cardPresenters.Sum(x => x.GetPriceByCount());
			_currentResourceCount.Value = _initialCount - totalPrice;
		
			_merchantShopPanel.FullPrice.SetCount(totalPrice);
			_merchantShopPanel.SubmitButton.Button.interactable = totalPrice > 0 && _resourcesService.GetCurrentResourceCount(_collectionResourceTypeCount) >= totalPrice;
			_merchantShopPanel.CurrentResourceCount.SetCount(_currentResourceCount.Value);
		}

		protected override void CreatePresenter(MerchantShopItemDataSo item)
		{
			var widget = _merchantShopPanel.MerchantItemCardWidget();
			var presenter = new SimpleMerchantCardPresenter(
					item.Model,
					widget,
					_itemStorage,
					_unlockService)
				.AddTo(_runtimeDisposable);
			
			presenter.Show(_currentResourceCount);
			presenter.CountInCart.Subscribe(OnCartCountChanged).AddTo(_runtimeDisposable);
			_cardPresenters.Add(presenter);
		}


		public override void OnClickSubmit(Unit obj)
		{
			var totalCount = _cardPresenters.Sum(x => x.GetPriceByCount());
			
			LucidAudio.PlaySE(_merchantShopPanel.DealSound)
				.SetVolume(0.5f)
				.SetSpatialBlend(0f);
			
			if (_resourcesService.GetCurrentResourceCount(_collectionResourceTypeCount) >= totalCount)
			{
				foreach (var cardPresenter in _cardPresenters)
				{
					if (cardPresenter.CountInCart.Value <= 0)
						continue;
					
					var message = new MessageObjectLocalPurchase(
						cardPresenter.Data.ObjectDataIdToBuy,
						cardPresenter.CountInCart.Value,
						_merchantShopItemPointManager);

					if (!string.IsNullOrEmpty(cardPresenter.Data.UnlockKey))
					{
						_unlockService.UnlockItem(cardPresenter.Data);
					}
					_localObjectPurchasePublisher.Publish(message);
					
					var isBought = _resourcesService.TrySpendResource(
						_collectionResourceTypeCount, 
						cardPresenter.GetPriceByCount(),
						new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, cardPresenter.Data.Id));
					
					Debug.Assert(isBought);
				}
				
				_runtimeDisposable?.Dispose();
				_onComplete();
			}
		}
		
		public override void Dispose()
		{
			_runtimeDisposable?.Dispose();
			_currentResourceCount?.Dispose();
		}
	}
}