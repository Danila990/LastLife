using Core.HealthSystem;

namespace Core.Entity.Repository
{
	public readonly struct MessageEntityDeath
	{
		public readonly DiedArgs DiedArgs;
		
		public MessageEntityDeath(DiedArgs diedArgs)
		{
			DiedArgs = diedArgs;
		}
	}
}