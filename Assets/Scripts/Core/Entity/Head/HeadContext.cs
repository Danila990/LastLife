using AnnulusGames.LucidTools.Audio;
using Core.AnimationRigging;
using Core.Entity.Ai;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic;
using Core.Entity.Repository;
using Core.Factory;
using Core.HealthSystem;
using Core.Inventory;
using Core.Loot;
using Core.Services.Experience;
using MessagePipe;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Head
{
	public class HeadContext : LifeEntity, IInteractableProvider, IControllableEntity, IExperienceEntity, ILootEntity
	{
		[TitleGroup("MainSettings")]
		[SerializeField] private CharacterHealth _health;
		[SerializeField] private BaseInventory _inventory;
		[SerializeField] private MonoRigProvider _rigProvider;
		[SerializeField] private Transform _aimTarget;
		[SerializeField] private Animator _animator;
		[TitleGroup("Base References")]
		[SerializeField] private OutlineHighLight _outlineHighLight;
		[SerializeField] private float _expOnDead = 1000f;
		private EntityTarget _selfTarget;
		private Rigidbody _rb;
		
		[TitleGroup("HeadData")]
		[field:SerializeField] public HeadData HeadData { get; private set; }
		public Renderer[] Renderers;

		public LootData LootData => HeadData.Loot;
		public override ILifeEntityHealth Health => _health;
		public BaseHeadAdapter CurrentAdapter { get; set; }
		public IEntityAdapter Adapter => CurrentAdapter;
		public IInventory Inventory => _inventory;
		public IRigProvider RigProvider => _rigProvider;
		public override Transform LookAtTransform => _aimTarget;
		public Animator Animator => _animator;
		public float ExperienceCount => _expOnDead;
		public IRagdollManager GetRagdollManager() => CurrentAdapter?.RagdollManager;
		public Vector3 GetExpPosition() => MainTransform.position + new Vector3(0, MeshProvider.MainRenderer.localBounds.max.y);
		public Vector3 GetPosition() => MainTransform.position;
		
		public override bool TryGetAiTarget(out IAiTarget result)
		{
			if (_health.IsDeath)
			{
				result = null;
				return false;
			}
			else
			{
				_selfTarget ??= new EntityTarget(this);
				result = _selfTarget;
				return true;
			}
		}

		public override void DoEffect(ref EffectArgs args)
		{
			CurrentAdapter.OnGetEffect(ref args);
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{			
			base.OnDestroyed(entityRepository);
			entityRepository.RemoveGenericEntity(this as LifeEntity);
			_selfTarget?.OnDestroy();
			CurrentAdapter.OnDestroyed();
		}

		protected override void OnCreated(IObjectResolver resolver)
		{
			Interactions.RemoveAll(x => x.InterfaceInteraction is EntityDestroyInteractable);
			_health.Init();
			_health.SetContext(this);
			_health.OnDeath.Subscribe(OnDeathInternal).AddTo(transform);
			_inventory.Initialize(this, resolver);
			AdditionalCameraDistance = 10;
		}
		
		public void OnStartFight()
		{
			LucidAudio
				.PlaySE(HeadData.SpawnSound)
				.SetPosition(MainTransform.position)
				.SetSpatialBlend(1f);
		}
		
		private void OnDeathInternal(DiedArgs _)
		{
			GetRagdollManager()?.Death();
			Animator.SetBool(AHash.Died, true);
			if (CurrentAdapter)
			{
				CurrentAdapter.OnDied();
			}
			
			LucidAudio
				.PlaySE(HeadData.DeathSound)
				.SetPosition(MainTransform.position)
				.SetSpatialBlend(1f)
				.Play();
			
			OnDeath();
		}

		protected virtual void OnDeath() {}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			_health.DoDamage(ref args);
			if (type is DamageType.Explosion or DamageType.Impact or DamageType.HardMelee && !_health.IsDeath)
			{
				Animator.SetTrigger(AHash.Impact);
			}
		}
		
		public void SetRb(Rigidbody rb)
		{
			_rb = rb;
		}
	}

	public interface ILootEntity
	{
		public LootData LootData { get; }
		public Vector3 GetPosition();
	}
}