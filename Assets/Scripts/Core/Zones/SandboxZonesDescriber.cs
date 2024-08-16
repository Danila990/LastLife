using Db.Map;
using MessagePipe;

namespace Core.Zones
{
	public class SandboxZonesDescriber : MapZonesDescriber
	{

		public SandboxZonesDescriber(ISubscriber<ZoneChangedMessage> sub) : base(sub)
		{
		}
		
		protected override void CreateHandlers()
		{
			Handlers[ZoneType.NoGuns] = new NoGunsHandler();
		}
	}
}
