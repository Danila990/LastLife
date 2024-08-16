using System;
using Analytic;
using Core.Equipment.Data;
using Core.Services;
using MessagePipe;
using VContainer.Unity;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class LocalItemPurchaseService : IInitializable, IDisposable
	{
		private readonly ISubscriber<MessageObjectLocalPurchase> _purchaseSubscriber;
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IAnalyticService _analyticService;
		private IDisposable _disposable;

		public LocalItemPurchaseService(
			ISubscriber<MessageObjectLocalPurchase> purchaseSubscriber,
			IItemStorage itemStorage,
			IItemUnlockService itemUnlockService,
			IAnalyticService analyticService
		)
		{
			_purchaseSubscriber = purchaseSubscriber;
			_itemStorage = itemStorage;
			_itemUnlockService = itemUnlockService;
			_analyticService = analyticService;
		}
		
		public void Initialize()
		{
			_disposable = _purchaseSubscriber.Subscribe(OnPurchase);
		}
		
		private void OnPurchase(MessageObjectLocalPurchase obj)
		{
			if (_itemStorage.All.TryGetValue(obj.BoughtItemId, out var objectData) && objectData is EquipmentItemData)
			{
				_itemUnlockService.UnlockItem(objectData);
				_analyticService.SendEvent($"LocalPurchase:Equipment:{obj.BoughtItemId}");
			}
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}