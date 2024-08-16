using System;
using System.Linq;
using AnnulusGames.LucidTools.Audio;
using Core.AnimationRigging;
using Core.Carry;
using Core.Effects;
using Core.Entity.Ai;
using Core.Entity.Characters.Adapters;
using Core.Entity.EntityAnimation;
using Core.Entity.EntityUpgrade;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.Equipment.Inventory;
using Core.HealthSystem;
using Core.Inventory;
using Cysharp.Threading.Tasks;
using FIMSpace.FProceduralAnimation;
using MessagePipe;
using PaintIn3D;
using RootMotion.Dynamics;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;
using Utils;
using VContainer;

namespace Core.Entity.Characters
{
	public interface IMeleeCharacter
	{
		public CharacterAnimatorAdapter AnimatorAdapter { get; }
		public StatsProvider StatsProvider { get; }
		public Transform MainTransform { get; }
		public CharacterAnimator CharacterAnimatorRef { get; }
		public uint Uid { get; }
	}
	
	public class CharacterContext : LifeEntity, IControllableEntity, IExperienceEntity, IMeleeCharacter
	{
		[TitleGroup("Adapter")]
		[field:ReadOnly, ShowInInspector] public BaseCharacterAdapter CurrentAdapter { get; private set;}
		[ReadOnly, NonSerialized, ShowInInspector] public bool IsOcupiedByAdapter;
		
		[TitleGroup("MainSettings")]
		[FormerlySerializedAs("Health")]
		[SerializeField] private CharacterHealth _health;
		
		[TitleGroup("Base References")]
		public CharacterAnimator CharacterAnimator;
		public Transform CharacterController;
		public OutlineHighLight CharacterOutline;
		public BehaviourPuppet BehaviourPuppet;
		public CharacterPartDamagable[] Limbs;
		
		[InlineButton("SetupPuppetMaster")] public PuppetMaster PuppetMaster;
		
		[field:SerializeField] public BaseInventory Inventory { get; private set; }
		[field:SerializeField] public EquipmentInventory EquipmentInventory { get; private set; }
		[field:SerializeField] public CarryInventory CarryInventory { get; private set; }
		
		[SerializeField] private MonoRigProvider _monoRigProvider;
		[SerializeField] private LegsAnimator _legsAnimator;
		[SerializeField] private Transform _lookAtTransform;
		[SerializeField] private UpgradeController _upgradeController;
		[field:SerializeField] public EffectsDataSo EffectsDataSo;
		public float _impactIgnoreTime;
		[TitleGroup("Custom References")]		
		public CharacterData CharacterData;
		public Renderer[] BodyRenders;
		public Material BodyMaterial;
		public Material LegMaterial;
		public bool IsFat;
		
		private CharacterRagDollManager _ragDollManager;
		private EntityTarget _selfTarget;
		private float _currentImpactIgnoreTime;
		private bool _immortal;
		private IObjectResolver _resolver;
		private IPublisher<MessageDamageEvent> _dmgPublisher;

		public IEntityAdapter Adapter => CurrentAdapter;
		public bool IsAimed => CurrentAdapter.AimController.CurrState is AimState.Aim or AimState.Sniper;
		public CharacterAnimatorAdapter AnimatorAdapter => CurrentAdapter.CharacterAnimatorAdapter;
		public StatsProvider StatsProvider => CurrentAdapter.StatsProvider;
		public override Transform MainTransform => CharacterController;
		public CharacterAnimator CharacterAnimatorRef => CharacterAnimator;
		public override Transform LookAtTransform => _lookAtTransform;
		public IRigProvider RigProvider => _monoRigProvider;
		public CharacterRagDollManager RagDollManager => _ragDollManager;
		public UpgradeController UpgradeController => _upgradeController;
		public LegsAnimator LegsAnimator => _legsAnimator;
		public float CurrentImpactIgnoreTime => _currentImpactIgnoreTime;
		public override ILifeEntityHealth Health => _health;
		public float ExperienceCount => 50f;
		
		
		protected override void OnCreated(IObjectResolver resolver)
		{
			CharacterOutline.Init();
			_resolver = resolver;
			_health.Init();
			_health.SetContext(this);
			_health.OnDeath.Subscribe(OnDeath).AddTo(transform);
			_health.AddTo(this);
			
			_ragDollManager = new CharacterRagDollManager(this);
			Inventory.Initialize(this, resolver);
			_dmgPublisher = resolver.Resolve<IPublisher<MessageDamageEvent>>();
			resolver.Inject(EquipmentInventory);
			resolver.Inject(CarryInventory);
			CarryInventory.Init(this);
			Enable().Forget();
		}

		private async UniTask Enable()
		{
			_legsAnimator.enabled = false;
			await UniTask.Delay(.35f.ToSec(), cancellationToken: destroyCancellationToken);
			_legsAnimator.enabled = true;
		}

		
		private void OnDeath(DiedArgs _)
		{
			CarryInventory.OnDeath();
			EquipmentInventory.Controller?.OnDeath();
			_ragDollManager.Death();	
			LucidAudio
				.PlaySE(CharacterData.DeathSound)
				.SetPosition(MainTransform.position)
				.SetSpatialBlend(1f)
				.Play();
		}
		
		public void SetImmortal(bool immortal)
		{
			_immortal = immortal;
			_health.SetImmortal(immortal);
		}

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
		
		public Vector3 GetExpPosition()
		{
			return MainTransform.position + new Vector3(0, MeshProvider.MainRenderer.localBounds.max.y);
		}

		public void SetAdapter(BaseCharacterAdapter adapter)
		{
			CurrentAdapter = adapter;
			Inventory.OnAdapterSet(adapter);
			EquipmentInventory.OnAdapterSet(adapter);
			CarryInventory.OnAdapterSet(adapter);
		}

		public void SetEquipmentStorage(AllEquipment allEquipment)
		{
			EquipmentInventory.Init(this, _resolver, allEquipment);
		}

		public override void DoEffect(ref EffectArgs args)
		{
			CurrentAdapter.OnGetEffect(ref args);
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			if (_immortal)
				return;
			_dmgPublisher.Publish(new MessageDamageEvent(args, this));
			_health.DoDamage(ref args);
		}

		public void DoDamageFromPart(ref DamageArgs args, PartType partType)
		{
			DoDamage(ref args, args.DamageType);
			if (partType == PartType.Body && !_health.IsDeath)
			{
				if (AudioService.TryPlayQueueSound(CharacterData.HurtSound, Uid.ToString(), 0.1f, out var player))
				{
					player
						.SetVolume(0.5f)
						.SetPosition(MainTransform.position)
						.SetSpatialBlend(1f);
				}
			}
		}

		protected override void SetContext(IInteractableContexted inter)
		{
			inter.SetCharContext(this);
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_selfTarget?.OnDestroy();
			entityRepository.RemoveGenericEntity(this);
			EquipmentInventory.OnDestroyed();
			CarryInventory.OnDestroyed();
			CurrentAdapter.OnContextDestroy();
		}

		public void PhysicImpact()
		{
			_currentImpactIgnoreTime = _impactIgnoreTime;
		}

		public void Update()
		{
			_currentImpactIgnoreTime -= Time.deltaTime;
		}

		public void DoMassDamage(ref DamageArgs args)
		{
			DoDamage(ref args, DamageType.Explosion);

			if (!_health.IsDeath)
			{
				if (AudioService.TryPlayQueueSound(CharacterData.HurtSound, Uid.ToString(), 0.1f, out var player))
				{
					player
						.SetVolume(0.5f)
						.SetPosition(MainTransform.position)
						.SetSpatialBlend(1f);
				}
				return;				
			}

			foreach (var limb in Limbs)
			{
				var newArgs = new DamageArgs(args.DamageSource, dismemberDamage: args.DismemberDamage,bloodLossAmount:0.01f,bloodLossTime:3);
				var behaviour = limb.BehaviourData.GetBehaviour(DamageType.ExplosionFromBody);
				behaviour.Apply(ref newArgs);
				limb.DismemberDamage(ref newArgs);
                
				args.DismemberDamage /= 1.4f;
				if (args.DismemberDamage <= 0)
				{
					return;
				}
			}
		}
		
#if UNITY_EDITOR
		[Button]
		private void SetUpReferences()
		{
			var parts = GetComponentsInChildren<ICharacterContextAcceptor>();
			foreach (var part in parts)
			{
				part.SetContext(this);
			}
			var parts1 = GetComponentsInChildren<IContextAcceptor>();
			foreach (var part in parts1)
			{
				part.SetContext(this);
			}
		}
		
		public void SetupPuppetMaster()
		{
			PuppetMaster.SetUpMuscles(CharacterController);
			Util.SetLayerRecursively(CharacterController.gameObject, Layers.CharacterLayer);
			UnityEditor.EditorUtility.SetDirty(PuppetMaster);
			UnityEditor.EditorUtility.SetDirty(CharacterController.gameObject);
		}
		
		[Button, PropertyOrder(-1), ShowIf("IsNeedInitialize")]
		private void EditorInitialize()
		{
			SetupPuppetMaster();
			CharacterAnimator.Animator = CharacterController.GetComponentInChildren<Animator>();
			CharacterAnimator.Animator.applyRootMotion = false;
			CharacterAnimator.Animator.gameObject.AddComponent<RigBuilder>();
			UnityEditor.EditorUtility.SetDirty(CharacterAnimator);
			UnityEditor.EditorUtility.SetDirty(CharacterAnimator.Animator);
			UnityEditor.EditorUtility.SetDirty(CharacterAnimator.Animator.gameObject);
			var rends = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var rend in rends)
			{
				if (rend && !rend.TryGetComponent(out P3dPaintable paintable))
				{
					rend.gameObject.AddComponent<P3dPaintable>();
					rend.gameObject.AddComponent<P3dMaterialCloner>();
					rend.gameObject.AddComponent<P3dPaintableTexture>();
					UnityEditor.EditorUtility.SetDirty(rend);
				}
			}


			_legsAnimator.Finding_LegBonesByNamesAndParenting();
			UnityEditor.EditorUtility.SetDirty(_legsAnimator);
			BodyRenders = rends;
			UnityEditor.EditorUtility.SetDirty(this);
		}

		private bool IsNeedInitialize()
		{
			return BodyRenders.Length == 0 || PuppetMaster.muscles.Any(muscle => !muscle.target) || !CharacterAnimator.Animator;
		}
#endif

	}
}