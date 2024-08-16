using VContainer;
using VContainer.Extensions;

namespace Ticket.Installer
{
	public class TicketsInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<TicketService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<TicketShopService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}