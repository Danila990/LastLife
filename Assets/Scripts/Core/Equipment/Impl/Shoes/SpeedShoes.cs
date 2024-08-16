using System;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;

namespace Core.Equipment.Impl.Shoes
{
	public class SpeedShoes : ShoesEntity
	{
		private IDisposable _stat;
		
		protected override void OnPutOnInternal()
		{
			base.OnPutOnInternal();
			if (Owner != null && Owner.Adapter is PlayerCharacterAdapter adapter)
			{
				_stat?.Dispose();
				_stat = adapter.StatsProvider.Stats.IncreaseStats(StatType.MovementSpeed, CurrentItemArgs.MovementSpeed);
			}
		}
		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_stat?.Dispose();
		}
		protected override void OnTakeOffInternal()
		{
			base.OnTakeOffInternal();
			_stat?.Dispose();
		}
	}
}
