using Core.Entity.Characters;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class PhysicEntityDamagable : EntityDamagable
	{
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private float _impactSpeed;
		[SerializeField] private float _impactDamage;
		[SerializeField] private bool _ignoreImapct;

		protected override void OnStart()
		{
			if (_rigidbody) return;
			_rigidbody = GetComponent<Rigidbody>();
		}

		public override void DoDamageBullet(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 bulletVel)
		{
			var bulDir = bulletVel.normalized;
			DoDamageWithEffects(ref args, pos, bulDir, DamageType.Range);
			Debug.DrawLine(pos, pos + bulDir * args.HitForce, Color.magenta, 1f);
			_rigidbody.AddForceAtPosition(bulDir * args.HitForce, pos, ForceMode.Acceleration);
		}

		public override void DoDamageExplosion(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 direction)
		{
			DoDamageWithEffects(ref args, pos, normal, DamageType.Explosion);
			Debug.DrawLine(pos, pos + direction * args.HitForce, Color.magenta, 1f);
			_rigidbody.AddForceAtPosition(direction * args.HitForce, pos, ForceMode.Acceleration);
		}

		protected void OnCollisionEnter(Collision collision)
		{
			if (_ignoreImapct) return;
			var impact = collision.impulse.magnitude / _rigidbody.mass;
			if (impact < _impactSpeed) return;
			var args = new DamageArgs(null, _impactDamage * (impact / _impactSpeed));
			var contact = collision.GetContact(0);
			base.DoDamageWithEffects(ref args, contact.point, contact.normal, DamageType.Impact);
		}
	}

}