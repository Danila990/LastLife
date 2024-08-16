using System;
using Core.ResourcesSystem.Interfaces;
using Ticket;

namespace Core.ResourcesSystem.Impl
{
	public class TicketResourceProvider : IResourceProvider
	{
		private readonly ITicketService _ticketService;
		public ResourceType ProviderType => ResourceType.Ticket;

		public TicketResourceProvider(ITicketService ticketService)
		{
			_ticketService = ticketService;
		}
		
		public int GetCurrentResourceCount()
		{
			return _ticketService.CurrentTicketsCount;
		}
		
		public IObservable<int> GetResourceObservable()
		{
			return _ticketService.TicketCountChanged;
		}
		
		public void AddResource(int amount)
		{
			_ticketService.OnPurchaseTickets(amount);
		}
		
		public bool TrySpendResource(int amount)
		{
			return _ticketService.TryUseTicketSilent(amount);
		}
	}

}