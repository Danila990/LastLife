using Core.Entity.Characters;
using Ticket;
using UnityEngine;
using VContainer;

namespace Core.Loot
{
	public class TicketLootEntity : LootEntity
	{
		[SerializeField] private int _quantity;

		[Inject] private readonly ITicketService _ticketService;
		
		protected override void OnInteractWithPlayer(CharacterContext context)
		{
			if (_quantity < 0)
			{
				Debug.LogWarning("_quantity < 0");
				return;
			}
			
			_ticketService.OnPurchaseTickets(_quantity);
		}
	}

}
