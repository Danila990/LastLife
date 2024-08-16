using Core.Entity.Characters;

namespace Core.Zones
{
	public abstract class ZoneHandler
	{
		private CharacterContext _currentContext;

		public void Handle(in ZoneChangedMessage zoneMsg)
		{
			_currentContext = zoneMsg.Context;
			OnHandle(in zoneMsg);
		}
		
		public void Reset()
		{
			if(_currentContext)
				OnResetContext(_currentContext);
			
			OnResetSelf();

			_currentContext = null;
		}
		
		protected abstract void OnHandle(in ZoneChangedMessage zoneMsg);
		
		protected abstract void OnResetContext(CharacterContext context);
		protected abstract void OnResetSelf();

	}

}
