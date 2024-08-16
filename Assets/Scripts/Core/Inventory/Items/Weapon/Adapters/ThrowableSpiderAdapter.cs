using UnityEngine;

namespace Core.Inventory.Items.Weapon.Adapters
{
	public class ThrowableSpiderAdapter : ThrowableAdapter
	{
		private Vector3 _targetPosition;
		
		public ThrowableSpiderAdapter(ThrowableWeaponContext context) : base(context)
		{
		}
		
		public override void Throw()
		{
			Context.ThrowObject();
		}
		
		public override Vector3 GetSpawnOriginPos()
		{
			var transform = Context.ShootingOrigin.GetOrigin("gun");
			return transform.position + transform.forward;
		}
		
		public void SetTargetPosition(Vector3 position)
			=> _targetPosition = position;
		
		public override Vector3 GetTargetPosition()
		{
			return _targetPosition;
		}
	}
}