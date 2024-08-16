using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Projectile
{
	public class ReboundExplosiveProjectile : ReboundProjectile
	{
		[SerializeField] private GameObject _view;
		[SerializeField] private AudioClip _explosionClip;
		[SerializeField] private AudioClip _beepClip;
		[ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
		[SerializeField] private string _explosionKey;
		[SerializeField] private float _radius;
		[SerializeField] private InternalExplosionVisitor _visitor;
        
		[Inject] private readonly IOverlapInteractionService _overlapInteractionService;

		private Transform _parent;
		private bool _isArmed;

		public override InteractionResultMeta Accept(EntityDamagable damageInteraction, ref InteractionCallMeta meta)
		{
			var res = base.Accept(damageInteraction, ref meta);

			if (_isArmed)
				return res;
			
			res = Rebound(ref res, ref meta);
			if (res.Interacted)
			{
				transform.SetParent(damageInteraction.transform);
				ArmAsync(meta).Forget();
			}
			return res;
		}
		public override InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
		{
			ArmAsync(meta).Forget();
			return base.Accept(environment, ref meta);
		}

		private async UniTaskVoid ArmAsync(InteractionCallMeta meta)
		{
			if(_isArmed)
				return;
			
			_isArmed = true;
			await UniTask.Delay(0.2f.ToSec(), cancellationToken: destroyCancellationToken);

			ArmSound();
            
			await UniTask.Delay(1f.ToSec(), cancellationToken: destroyCancellationToken);
            
			Explode();
		}

		protected override void SetOwner(EntityContext context)
		{
			base.SetOwner(context);
			_visitor.SetOwner(this);
		}

		private void Explode()
		{
			ExplosionVFX();
			ExplosionSound();
			DoExplode();
		}

		private void DoExplode()
		{
			_view.SetActive(false);
			_overlapInteractionService.OverlapSphere(_visitor, _view.transform.position, _radius, Uid);
		}

		public override void OnRent()
		{
			base.OnRent();
			_view.SetActive(true);
			_isArmed = false;
			_parent = transform.parent;
		}
		
		protected override void OnReturnToPool()
		{
			_isArmed = false;
			if(transform.parent != _parent)
				transform.SetParent(_parent);
		}
		
		private void ArmSound()
		{
			LucidAudio.PlaySE(_beepClip)
					.SetPosition(_view.transform.position)
					.SetSpatialBlend(1f)
					.SetVolume(0.1f);
		}

		private void ExplosionVFX()
		{
			VFXFactory.CreateAndForget(_explosionKey, _view.transform.position);
		}

		private void ExplosionSound()
		{
			if (AudioService.TryPlayQueueSound(_explosionClip, Uid.ToString(), 0.1f, out var player))
			{
				player
					.SetPosition(_view.transform.position)
					.SetSpatialBlend(1f)
					.SetVolume(0.4f);
			}
		}
        
	}
}
