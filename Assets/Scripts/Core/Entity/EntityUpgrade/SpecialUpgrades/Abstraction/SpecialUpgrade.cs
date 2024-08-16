using System;
using Core.Inventory;

namespace Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction
{
	public abstract class SpecialUpgrade : ISpecialUpgrade, IDisposable
	{
		public abstract void Initialize(BaseInventory inventory);
		public abstract void OnLevelChanged(int level);
		public abstract void Dispose();
	}

	public interface ISpecialUpgrade
	{
		void Initialize(BaseInventory inventory);
		void OnLevelChanged(int level);
	}
}