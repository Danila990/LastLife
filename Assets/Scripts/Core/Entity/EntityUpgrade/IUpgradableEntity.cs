using Core.Inventory.Items.Weapon;

namespace Core.Entity.EntityUpgrade
{
	public interface IUpgradableEntity
	{
		public ref UpgradeableParameters Parameters { get; }
		public void ResetUpgrades();

		public void OnUpgradesInit();
		public void OnUpgrade();
	}
}