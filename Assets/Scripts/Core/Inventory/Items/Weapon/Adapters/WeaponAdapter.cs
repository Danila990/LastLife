using System;

namespace Core.Inventory.Items.Weapon.Adapters
{
	public abstract class WeaponAdapter<T> : IWeaponAdapter, IDisposable
		where T : WeaponContext
	{
		public T Context { get; }
		public WeaponAdapter(T context)
		{
			Context = context;
		}
		public virtual void Dispose()
		{
		}
	}

	public interface IWeaponAdapter
	{
		
	}
}