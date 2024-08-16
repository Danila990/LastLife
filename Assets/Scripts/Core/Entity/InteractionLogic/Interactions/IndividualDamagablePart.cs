using Core.Entity.Characters;
using Core.HealthSystem;
using UniRx;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{

	public class IndividualDamagablePart : EntityDamagable, IHealthProvider
	{
		public Health Health;

		public AudioClip OnDeathClip;
		public float MaxDamage;
		public Transform FxSpawnPoint;
		public ParticleSystem CustomFX;
		
		private void Awake()
		{
			Health.Init();
			Health.AddTo(TargetContext);
			Health.OnDeath.Subscribe(OnDeath).AddTo(TargetContext);
		}
		
		public IHealth GetHealth() => Health;

		private void OnDeath(DiedArgs obj)
		{
			CustomFX.Play();
			if (OnDeathClip)
			{
				TargetContext
					.AudioService
					.PlayNonQueue(OnDeathClip)
					.SetSpatialBlend(1f)
					.SetPosition(transform.position);
			}
			gameObject.SetActive(false);
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			var behaviour = BehaviourData.GetBehaviour(type);
			behaviour.Apply(ref args);
			if (MaxDamage > 0)
			{
				args.Damage = Mathf.Clamp(args.Damage, 0, MaxDamage);
			}
			Health.DoDamage(ref args);
		}
		
	}

	public interface IHealthProvider
	{
		IHealth GetHealth();
	}
}