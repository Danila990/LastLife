using Ticket;
using UniRx;
using Utils;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.TicketUI
{
    public class TicketCountController : UiController<TicketCountView>, IStartable
    {
        private readonly ITicketService _ticketService;
    
        public TicketCountController
        (
            ITicketService ticketService
        )
        {
            _ticketService = ticketService;
        }
    
        public void Start()
        {
            _ticketService.TicketCountChanged.SubscribeToText(View.Count).AddTo(View);
        }
    }

}