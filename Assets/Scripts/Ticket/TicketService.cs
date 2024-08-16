using System;
using Adv.Messages;
using Analytic;
using MessagePipe;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;

namespace Ticket
{
	public interface ITicketService
	{
		int CurrentTicketsCount { get; }
		IObservable<int> TicketCountChanged { get; }
		bool TryUseTicket(Action onUse, string meta = "");
		bool TryUseTicketSilent(int amount);
		void OnPurchaseTickets(int ticketCount);
	}
	
	public class TicketService : ITicketService, IDisposable
	{
		private readonly IAnalyticService _analyticService;
		private readonly IPublisher<ShowShopMenu> _publisher;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly IntReactiveProperty _currentTicketsCount;
		private const string TICKET_KEY = "TICKET_KEY";

		public IObservable<int> TicketCountChanged => _currentTicketsCount;
		
		[ShowInInspector]
		public int CurrentTicketsCount
		{
			get => _currentTicketsCount.Value;
			private set
			{
				PlayerPrefs.SetInt(TICKET_KEY, value);
				_currentTicketsCount.Value = value;
			}
		}
		
		public TicketService(
			IAnalyticService analyticService,
			IPublisher<ShowShopMenu> publisher
				)
		{
			_analyticService = analyticService;
			_publisher = publisher;
			_currentTicketsCount = new IntReactiveProperty(PlayerPrefs.GetInt(TICKET_KEY, 0));
		}

		public bool TryUseTicketSilent(int amount)
		{
			if (CurrentTicketsCount >= amount)
			{
				CurrentTicketsCount -= amount;
				return true;
			}
			return false;
		}
		
		public void OnPurchaseTickets(int ticketCount)
		{
			CurrentTicketsCount += ticketCount;
		}

		public bool TryUseTicket(Action onUse, string meta = "")
		{
			meta = "TicketUsage:" + meta;
			if (CurrentTicketsCount > 0)
			{
				UseTicket(onUse, meta);
				return true;
			}
			else
			{
				//Debug.Log("Ticket Ends");
				_publisher.Publish(new ShowShopMenu("tickets"));
				return false;
			}
		}

		private void UseTicket(Action action, string meta)
		{
			CurrentTicketsCount--;
			action();
			_analyticService.SendEvent(meta);
			Debug.Log( "Used Ticket for :".SetColor("yellow") + $" [{meta.SetColor()}]");
		}
		
		public void Dispose()
		{
			_currentTicketsCount?.Dispose();
			_compositeDisposable?.Dispose();
		}
	}
}