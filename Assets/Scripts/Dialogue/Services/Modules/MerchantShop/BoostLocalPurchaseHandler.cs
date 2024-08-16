using System;
using Analytic;
using Core.Factory;
using Core.InputSystem;
using Core.Services;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace Dialogue.Services.Modules.MerchantShop
{

	public class BoostLocalPurchaseHandler : IInitializable, IDisposable
	{
		private readonly ISubscriber<MessageObjectLocalPurchase> _purchaseSubscriber;
		private readonly IItemStorage _itemStorage;
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IAnalyticService _analyticService;
		private IDisposable _disposable;
		
		public BoostLocalPurchaseHandler(
			ISubscriber<MessageObjectLocalPurchase> purchaseSubscriber,
			IItemStorage itemStorage,
			IPlayerSpawnService playerSpawnService,
			IAnalyticService analyticService
			)
		{
			_purchaseSubscriber = purchaseSubscriber;
			_itemStorage = itemStorage;
			_playerSpawnService = playerSpawnService;
			_analyticService = analyticService;
		}
		
		public void Initialize()
		{
			_disposable = _purchaseSubscriber.Subscribe(OnPurchase);
		}
		
		private void OnPurchase(MessageObjectLocalPurchase msg)
		{
			if (_itemStorage.BoostObjects.TryGetValue(msg.BoughtItemId, out var item))
			{
				Debug.Assert(msg.Quantity > 0);

				_playerSpawnService.PlayerCharacterAdapter.BoostsInventory.Add(item.BoostArgs, msg.Quantity);
				_analyticService.SendEvent($"LocalPurchase:Boost:{msg.BoughtItemId}");
			}
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}