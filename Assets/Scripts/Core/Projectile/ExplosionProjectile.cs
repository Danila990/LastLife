using Core.Entity;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Projectile.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Projectile
{
	public class ExplosionProjectile : SimpleProjectile
	{
		[SerializeField] private bool _ownByOwner;
		[SerializeField] private InternalExplosionVisitor _visitor;
		[SerializeField] private GameObject _view;
		[SerializeField] private float _radius;
		[SerializeField] private AudioClip _explosionClip;
		[SerializeField] private ParticleSystem[] _particleSystems;
		
		[ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
		[SerializeField] private string _explosionKey;
		
		[Inject] private readonly IOverlapInteractionService _overlapInteractionService;
		
		public override InteractionResultMeta Accept(EntityDamagable damageInteraction, ref InteractionCallMeta meta)
		{
			if (damageInteraction.TargetContext.Uid.Equals(Owner.Uid))
				return StaticInteractionResultMeta.Default;
			if (damageInteraction.TargetContext is ShieldEntityContext)
				_visitor.NoDamage = true;
			return StaticInteractionResultMeta.InteractedBlocked;
		}
		
		public override InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
		{
			if (environment.Uid.Equals(Owner.Uid))
				return StaticInteractionResultMeta.Default;

			return base.Accept(environment, ref meta);
		}
		
		public override void OnRent()
		{
			base.OnRent();
			_visitor.NoDamage = false;
			_view.SetActive(true);
			SetEmission(true);
		}

		protected override void SetOwner(EntityContext context)
		{
			base.SetOwner(context);
			_visitor.SetOwner(_ownByOwner ? context : this);
		}

		public override void OnCollision(IProjectileService service)
		{
			Explode();
			service.ReleaseProjectile(_currentLink);
		}

		private void Explode()
		{
			ExplosionVFX();
			ExplosionSound();
			DoExplode();
		}
		
		private void DoExplode()
		{
			_visitor.ExtraDmg = AdditionalDamage;
			_view.SetActive(false);
			_overlapInteractionService.OverlapSphere(_visitor, transform.position, _radius, Uid);
			SetEmission(false);
			Particle?.Stop();
		}
		
		private void SetEmission(bool state)
		{
			foreach (var system in _particleSystems)
			{
				var systemEmission = system.emission;
				systemEmission.enabled = state;
			}
		}
		

		private void ExplosionSound()
		{
			if (AudioService.TryPlayQueueSound(_explosionClip, Uid.ToString(), 0.1f, out var player))
			{
				player
					.SetPosition(transform.position)
					.SetSpatialBlend(1f)
					.SetVolume(0.4f);
			}
		}

		private void ExplosionVFX()
		{
			VFXFactory.CreateAndForget(_explosionKey, transform.position);
		}
	}
}