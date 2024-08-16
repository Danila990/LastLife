using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using VContainer.Unity;

namespace Market.Bank
{
	public interface IBankService
	{
		bool TryBuyGoldTickets(int goldTicketsAmount);
		bool TrySellGoldTickets(int goldTicketsAmount);
	}
	
	public class BankService : IBankService
	{
		private readonly IResourcesService _resourceService;
		private const int EXCHANGE_RATE = 10;
		
		public BankService(IResourcesService resourceService)
		{
			_resourceService = resourceService;
		}

		public bool TryBuyGoldTickets(int amount)
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, ResourceItemIds.BANKER_ITEM_ID);

			if (_resourceService.TrySpendResource(ResourceType.Ticket, amount * EXCHANGE_RATE, meta))
			{
				_resourceService.AddResource(ResourceType.GoldTicket, amount, meta);
				return true;
			}

			return false;
		}
		
		public bool TrySellGoldTickets(int amount)
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.MERCHANT_ITEM_TYPE, ResourceItemIds.BANKER_ITEM_ID);

			if (_resourceService.TrySpendResource(ResourceType.GoldTicket, amount, meta))
			{
				_resourceService.AddResource(ResourceType.Ticket, amount * EXCHANGE_RATE, meta);
				return true;
			}

			return false;
		}
	}
}