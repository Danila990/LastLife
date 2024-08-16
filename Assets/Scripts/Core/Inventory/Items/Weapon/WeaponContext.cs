using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items.Weapon.Adapters;

namespace Core.Inventory.Items.Weapon
{
	public abstract class WeaponContext : ItemContext
	{
		
	}

	public abstract class WeaponContextWithAdapter<TAdapter> : WeaponContext
		where TAdapter : IWeaponAdapter
	{
		public TAdapter WeaponAdapter;

		public override void ItemInit(IOriginProxy inventory)
		{
			base.ItemInit(inventory);
			WeaponAdapter = TryGetWeaponAdapter(Owner, (Owner as IControllableEntity)?.Adapter);
		}

		protected abstract TAdapter TryGetWeaponAdapter(EntityContext owner, IEntityAdapter adapter);
	}
}