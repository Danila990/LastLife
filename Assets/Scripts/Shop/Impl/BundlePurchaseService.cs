using System;
using System.Collections.Generic;
using Core.Services;
using MessagePipe;
using Shop.Abstract;
using Shop.Messages;
using Shop.Models;
using VContainer.Unity;

namespace Shop.Impl
{
	public class BundlePurchaseService : IInitializable, IDisposable
	{
		private readonly ISubscriber<PurchaseMessage> _purchaseMessage;
		private readonly IShopData _shopData;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly SortedList<string, BundleShopItemModel> _inAppIdToObjectId = new SortedList<string, BundleShopItemModel>();

		private IDisposable _disposable;

		public BundlePurchaseService(
			ISubscriber<PurchaseMessage> purchaseMessage, 
			IShopData shopData,
			IItemUnlockService itemUnlockService
		)
		{
			_purchaseMessage = purchaseMessage;
			_shopData = shopData;
			_itemUnlockService = itemUnlockService;
		}
		
		public void Initialize()
		{
			_disposable = _purchaseMessage.Subscribe(OnItemPurchase);
			foreach (var boost in _shopData.BundleScriptable)
			{
				_inAppIdToObjectId.Add(boost.Model.InAppId, boost.Model);
			}
		}
		
		private void OnItemPurchase(PurchaseMessage obj)
		{
			if (_inAppIdToObjectId.TryGetValue(obj.Product.InAppID, out var item))
			{
				_itemUnlockService.UnlockBundle(item);
			}
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}