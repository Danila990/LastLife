using UnityEngine;

namespace Core.Inventory.Items.Weapon.Adapters
{
	public abstract class ThrowableAdapter : WeaponAdapter<ThrowableWeaponContext>
	{
		public bool InAttack { get; protected set; }

		protected ThrowableAdapter(ThrowableWeaponContext context) : base(context)
		{
		}
		public abstract void Throw();
		public abstract Vector3 GetSpawnOriginPos();
		public abstract Vector3 GetTargetPosition();
	}
}