using Core.Entity.InteractionLogic;
using Core.HealthSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Projectile
{
	public class RevolverProjectile : SimpleProjectile
	{
		[TitleGroup("Revolver")]
		public float NotEffectiveDistance = 5f;
		
		protected override void AffectDamage(ref DamageArgs args, ref InteractionCallMeta meta)
		{
			var distanceFromOrigin = Vector3.Distance(CurrentLink.CreationData.Position, meta.Point);
			if (distanceFromOrigin > NotEffectiveDistance)
			{
				args.Damage *= 2f;
			}
		}
	}
}