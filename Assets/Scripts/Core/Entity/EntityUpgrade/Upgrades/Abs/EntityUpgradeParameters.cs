using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Entity.EntityUpgrade.Upgrades.Abs
{
	public abstract class EntityUpgradeParameters : IEntityUpgrade
	{
		[field:SerializeField] public string Name { get; private set; }
		public abstract string PrefKey { get; }
		public int MaxLevel => 5;
		public abstract void ApplyUpgrade(ref UpgradeableParameters parameters, int level);
	}

	public interface IEntityUpgrade
	{
		string PrefKey { get; }
		string Name { get; }
		public void ApplyUpgrade(ref UpgradeableParameters parameters, int level);
	}
}