using System;
using System.Collections.Generic;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using MessagePipe;
using Shop.Abstract;
using Shop.Messages;
using Shop.Models;
using VContainer.Unity;

namespace Ticket
{
	public class TicketShopService : IInitializable, IDisposable
	{
		private readonly ISubscriber<PurchaseMessage> _purchaseMessage;
		private readonly IShopData _shopData;
		private readonly IResourcesService _resourcesService;
		private readonly Dictionary<string, TicketModel> _ticketModels = new Dictionary<string, TicketModel>();
		private IDisposable _disposable;

		public TicketShopService(
			ISubscriber<PurchaseMessage> purchaseMessage, 
			IShopData shopData,
			IResourcesService resourcesService)
		{
			_purchaseMessage = purchaseMessage;
			_shopData = shopData;
			_resourcesService = resourcesService;
		}
		
		public void Initialize()
		{
			foreach (var shopData in _shopData.TicketShopItems)
			{
				_ticketModels.Add(shopData.Model.InAppId, shopData.Model);
			}
			_disposable = _purchaseMessage.Subscribe(Handler);
		}
		
		private void Handler(PurchaseMessage obj)
		{
			if (_ticketModels.TryGetValue(obj.Product.InAppID, out var ticketModel))
			{
				var meta = new ResourceEventMetaData(ResourceItemTypes.SHOP_ITEM_TYPE, ticketModel.InAppId);
				_resourcesService.AddResource(ticketModel.ResourceType, ticketModel.TicketCount, meta);
			}
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}