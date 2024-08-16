using Core.HealthSystem;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class KinematicEntityDamagable : EntityDamagable
	{
		public Rigidbody Rb;
		public float Mlp = 1f;
		
		public override void DoDamageMelee(ref DamageArgs args, Vector3 pos, Vector3 normal)
		{
			DoDamageWithEffects(ref args, pos, normal, args.DamageType);

			if (!Rb.isKinematic)
			{
				Rb.AddForceAtPosition(-normal * (args.HitForce * Mlp), pos, ForceMode.Acceleration);
			}
		}
	}
}