using Common;
using Core.Entity.Characters;
using Core.HealthSystem;
using GameSettings;
using RootMotion.Dynamics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Entity.InteractionLogic.Interactions
{

	public class CharacterPartDamagable : EntityDamagable, ICharacterContextAcceptor
	{
		[SerializeField] private CharacterContext _characterContext;
		[SerializeField] private PartType PartType;
		[SerializeField] private bool _bloodLoss;
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private float _impactSpeed;
		[SerializeField] private float _impactDamage;
		[SerializeField] private float _dismemberMaxHealth;
		[SerializeField] private bool _impactAnimation;
		private MuscleCollisionBroadcaster _broadcaster;
		private float _dismemberHealth;
		private bool _isConnected = true;
		public bool IsConnected => _isConnected;
		public CharacterContext CharacterContext => _characterContext;
		public void SetContext(CharacterContext context)
		{
			_characterContext = context;
		}

		private void OnValidate()
		{
			DontCashDamagedUIDs = true;
		}

		protected override void OnStart()
		{
			_broadcaster = GetComponent<MuscleCollisionBroadcaster>();
			if (_rigidbody) return;
			_rigidbody = GetComponent<Rigidbody>();
			_dismemberHealth = _dismemberMaxHealth;
		}

		public override void DoDamageWithEffects(ref DamageArgs args, Vector3 pos, Vector3 normal, DamageType type)
		{
			base.DoDamageWithEffects(ref args, pos, normal, type);
			if (_impactAnimation)
			{
				_characterContext.CharacterAnimator.Impact();
			}

            if (args.BloodLossAmount <= 0)
                DrawBurns(pos);
			else
				DrawBlood(pos);
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			if (_dismemberHealth < 0)
				return;

			var behaviour = BehaviourData.GetBehaviour(type);
			behaviour.Apply(ref args);
			_characterContext.DoDamageFromPart(ref args, PartType);
			DismemberDamage(ref args);
		}

		public void DismemberDamage(ref DamageArgs args)
		{
			if (_characterContext.Health.IsImmortal)
				return;
			_dismemberHealth -= args.DismemberDamage;

			if (_dismemberHealth < 0 && _isConnected)
			{
				_isConnected = false;
				_characterContext.PuppetMaster.DisconnectMuscleRecursive(_broadcaster.muscleIndex);
                if (args.BloodLossAmount > 0)
                    _characterContext.Health.StartBloodLoss(ref args, transform.position, -transform.forward, transform);
                _rigidbody.AddForceAtPosition(Random.insideUnitSphere * args.HitForce, transform.position, ForceMode.Acceleration);

				if (PartType == PartType.Legs)
				{
					_characterContext.Health.ForceDeath();
					_characterContext.RagDollManager.Death();
				}
			}
		}

		public override void DoDamageBullet(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 bulletVel)
		{
			var bulDir = bulletVel.normalized;
			DoDamageWithEffects(ref args, pos, normal, DamageType.Range);
            if (args.BloodLossAmount <= 0)
                return;
            if (PartType != PartType.Head)
			{
				_broadcaster.Hit(args.Unpin * 10, bulDir * (100 * args.HitForce), pos);
			}
			TryEnableBloodLoss(ref args, pos, normal);
		}

		private void TryEnableBloodLoss(ref DamageArgs args, Vector3 pos, Vector3 normal)
		{
			if (!_bloodLoss || _characterContext.Health.CurrentBloodLevel < 0)
				return;
			_characterContext.Health.StartBloodLoss(ref args, pos, normal, transform);
		}

		public override void DoDamageExplosion(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 direction)
		{
			DoDamageWithEffects(ref args, pos, normal, DamageType.Explosion);
			
			if(_rigidbody)
				_rigidbody.AddForceAtPosition(direction * args.HitForce, pos, ForceMode.Acceleration);
		}

		public override void DoDamageMelee(ref DamageArgs args, Vector3 pos, Vector3 normal)
		{
			DoDamageWithEffects(ref args, pos, normal, args.DamageType);
		
			_broadcaster.Hit(args.Unpin * 10, -normal * (100 * args.HitForce), pos);
		}

		protected void OnCollisionEnter(Collision collision)
		{
			if (!_rigidbody)
				return;
			
			var impact = collision.impulse.magnitude / _rigidbody.mass;
			
			if (impact < _impactSpeed)
				return;
			
			if (_characterContext.CurrentImpactIgnoreTime > 0)
				return;
			
			_characterContext.PhysicImpact();
			var args = new DamageArgs(null, _impactDamage * (impact / _impactSpeed), dismemberDamage: _impactDamage * (impact / _impactSpeed))
			{
				MetaDamageSource = new MetaDamageSource("Collision")
			};
			var contact = collision.GetContact(0);
            DrawBlood(contact.point);
            base.DoDamageWithEffects(ref args, contact.point, contact.normal, DamageType.Impact);
		}

		private void DrawBlood(Vector3 pos)
		{
			if (!_bloodLoss)
				return;
			if (GameSetting.ViolenceStatus) 
				return;
			PaintLink.Instance.ChangeColor(false);
			PaintLink.Instance.HandleDecal(pos, Quaternion.LookRotation(Random.insideUnitSphere));
		}

		private void DrawBurns(Vector3 pos)
		{
            if (GameSetting.ViolenceStatus)
                return;
            PaintLink.Instance.ChangeColor(Color.black);
            PaintLink.Instance.HandleDecal(pos, Quaternion.LookRotation(Random.insideUnitSphere));
        }
	}

}