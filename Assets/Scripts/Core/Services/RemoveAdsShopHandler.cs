using System;
using System.Collections.Generic;
using Adv.Services;
using Adv.Services.Interfaces;
using MessagePipe;
using Shop.Abstract;
using Shop.Messages;
using VContainer.Unity;

namespace Core.Services
{
	public class RemoveAdsShopHandler : IInitializable, IDisposable
	{
		private readonly ISubscriber<PurchaseMessage> _purchaseMessage;
		private readonly IShopData _shopData;
		private readonly IRemoveAdsService _removeAdsService;
		private readonly SortedList<string, IRemoveAdsModel> _removeAdsModels = new SortedList<string, IRemoveAdsModel>();
		private IDisposable _disposable;

		public RemoveAdsShopHandler(
			ISubscriber<PurchaseMessage> purchaseMessage,
			IShopData shopData,
			IRemoveAdsService removeAdsService)
		{
			_purchaseMessage = purchaseMessage;
			_shopData = shopData;
			_removeAdsService = removeAdsService;
		}
		
		public void Initialize()
		{
			foreach (var model in _shopData.GetAllShopItemModels())
			{
				if (model is IRemoveAdsModel removeAdsModel)
				{
					_removeAdsModels.Add(model.InAppId, removeAdsModel);
				}
			}
			_disposable = _purchaseMessage.Subscribe(Handler);
		}
		
		private void Handler(PurchaseMessage msg)
		{
			if (_removeAdsModels.TryGetValue(msg.Product.InAppID, out var removeAdsModel) && removeAdsModel.IsRemoveAds)
			{
				_removeAdsService.RemoveAds(removeAdsModel.ConstantlyRemoveAds);
			}
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}