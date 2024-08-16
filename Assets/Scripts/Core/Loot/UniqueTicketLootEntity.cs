using Core.Entity.Characters;
using Ticket;
using UnityEngine;
using VContainer;

namespace Core.Loot
{
	public class UniqueTicketLootEntity : UniqueLootEntity
	{
		[SerializeField] private int _quantity;

		[Inject] private ITicketService _ticketService;

		protected override void OnInteractWithPlayer(CharacterContext context)
		{
			if(_quantity <= 0)
				return;
			SetInPrefs();
			_ticketService.OnPurchaseTickets(_quantity);
		}
	}
}
