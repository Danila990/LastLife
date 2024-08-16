using System;
using System.Collections.Generic;
using Core.Services;
using Db.ObjectData;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using Shop.Abstract;
using Shop.ItemProvider;
using Shop.Messages;
using Shop.Models.Product;
using Utils.Constants;
using VContainer.Unity;

namespace Shop.Impl
{
	public interface IItemPurchaseService
	{
		void TryUnlockIAP(InAppProduct product);
	}
	
	public class ItemPurchaseService : IItemPurchaseService, IInitializable, IDisposable
	{
		private readonly ISubscriber<PurchaseMessage> _purchaseMessage;
		private readonly IShopData _shopData;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly SortedList<string, ObjectData> _inAppIdToObjectId = new SortedList<string, ObjectData>();
		private IDisposable _disposable;

		public ItemPurchaseService(
			ISubscriber<PurchaseMessage> purchaseMessage, 
			IShopData shopData,
			IItemUnlockService itemUnlockService,
			IPlayerPrefsManager playerPrefsManager
			)
		{
			_purchaseMessage = purchaseMessage;
			_shopData = shopData;
			_itemUnlockService = itemUnlockService;
			_playerPrefsManager = playerPrefsManager;
		}
		
		public void Initialize()
		{
			_disposable = _purchaseMessage.Subscribe(OnItemPurchase);
			AddItems(_shopData.CharacterShopItems);
			AddItems(_shopData.ShopBulletproofItems);
			AddItems(_shopData.ShopJetpackItems);
			AddItems(_shopData.ShopBootsItems);
			AddItems(_shopData.ShopHatItems);
		}

		private void AddItems<T>(IEnumerable<ScriptableInvItem<T>> shopDataCharacterShopItems) where T : ObjectData
		{
			foreach (var shopItem in shopDataCharacterShopItems)
			{
				_inAppIdToObjectId.Add(shopItem.Model.InAppId, shopItem.Model.Item.ObjectData);
				//_inAppIdToShopModel.Add(shopItem.Model.InAppId, shopItem.Model);
				
				if (_itemUnlockService.IsPurchased(shopItem.Model))
				{
					SetSkipInter();
				}
			}
		}

		private void OnItemPurchase(PurchaseMessage product)
		{
			TryUnlockIAP(product.Product);
			SetSkipInter();
		}
		
		public void TryUnlockIAP(InAppProduct product)
		{
			if (_inAppIdToObjectId.TryGetValue(product.InAppID, out var objectData))
			{
				_itemUnlockService.UnlockItemOnPurchase(objectData, product);
			}
		}

		private void SetSkipInter()
		{
			_playerPrefsManager.SetValue(Consts.SKIP_INTERSTITIAL, true);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}