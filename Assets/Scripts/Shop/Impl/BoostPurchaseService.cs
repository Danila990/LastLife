using System;
using System.Collections.Generic;
using Core.InputSystem;
using Db.ObjectData;
using MessagePipe;
using Shop.Abstract;
using Shop.Messages;
using Shop.Models;
using VContainer.Unity;

namespace Shop.Impl
{
	public class BoostPurchaseService : IInitializable, IDisposable
	{
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly ISubscriber<PurchaseMessage> _purchaseMessage;
		private readonly IShopData _shopData;
		private readonly SortedList<string, ConsumableItemModel<BoostObjectData>> _inAppToBoost = new SortedList<string, ConsumableItemModel<BoostObjectData>>();
		private IDisposable _disposable;

		public BoostPurchaseService(
			IPlayerSpawnService playerSpawnService,
			ISubscriber<PurchaseMessage> purchaseMessage, 
			IShopData shopData)
		{
			_playerSpawnService = playerSpawnService;
			_purchaseMessage = purchaseMessage;
			_shopData = shopData;
		}
		
		public void Initialize()
		{
			foreach (var boost in _shopData.BoostsItems)
			{
				_inAppToBoost.Add(boost.Model.InAppId, boost.Model);
			}
			_disposable = _purchaseMessage.Subscribe(OnItemPurchase);
		}
		
		private void OnItemPurchase(PurchaseMessage product)
		{
			if (_inAppToBoost.TryGetValue(product.Product.InAppID, out var boost))
			{
				_playerSpawnService.PlayerCharacterAdapter.BoostsInventory.Add(boost.Item.Model.BoostArgs, boost.Count);
			}
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}