using Core.Boosts.Impl;
using Core.Entity.Ai;
using Core.Entity.Ai.Movement;
using Core.Entity.Ai.Ragdoll;
using Core.Entity.Ai.Sensor.InventorySensor;
using Core.Entity.Ai.TargetFinder;
using Core.Entity.EntityAnimation;
using Core.Entity.Head;
using Core.Entity.Repository;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using LitMotion;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Characters.Adapters
{
	public class AiHeadAdapter : BaseHeadAdapter
	{
		[SerializeField] protected AiMovementController _aiMovementController;
		[SerializeField] protected Blackboard _aiBlackboard;
		[SerializeField] private BlackboardAiInventorySensor _aiInventorySensor;
		[SerializeField] protected BehaviourTreeOwner _behaviourTreeOwner;
		[SerializeField] private AiTargetSelector _targetSelector;
		[SerializeField] private SimpleAnimatorAdapter _simpleAnimatorAdapter;

		private NavMeshRagdoll _meshRagdoll;
		private IAiTarget _aiTarget;
		private IEntityRepository _entityRepository;
		protected IObjectResolver Resolver;

		public override IRagdollManager RagdollManager => _meshRagdoll;
		public BlackboardAiInventorySensor AiInventorySensor => _aiInventorySensor;
		public BehaviourTreeOwner BehaviourTreeOwner => _behaviourTreeOwner;
		public Blackboard AiBlackboard => _aiBlackboard;
		public override AnimatorAdapter AnimatorAdapter => _simpleAnimatorAdapter;
		public override Transform MainAdapterTransform => transform;
		public override IBoostProvider BoostProvider => null;
		public override StatsProvider StatsProvider => null;

		
		[Inject] 
		private void Construct(IEntityRepository entityRepository, IObjectResolver resolver)
		{
			Resolver = resolver;
			_entityRepository = entityRepository;
			_aiInventorySensor.SetAdapter(this);
			if (_aiMovementController)
			{
				_aiMovementController.Created(resolver);
			}
			resolver.Inject(_aiInventorySensor);
			OnConstruct();
		}
		
		protected virtual void OnConstruct() { }

		public override void SetEntityContext(HeadContext context)
		{
			base.SetEntityContext(context);
			context.transform.SetParent(transform);
			_aiBlackboard.SetVariableValue("MoveSpeed", context.HeadData.MoveSpeed);
			_aiBlackboard.SetVariableValue("RigProvider", context.RigProvider);
			context.Health.OnDamage.Subscribe(OnGetDamage).AddTo(this);
			
			context.SetRb(GetComponent<Rigidbody>());
			_aiInventorySensor.SetInv(context.Inventory);
			_aiInventorySensor.ObserveInventory();

			if (_aiMovementController is NavmeshAgentController aiMovementController)
			{
				aiMovementController.TargetAnimator = context.Animator;
			}
		}
		
		protected virtual void OnGetDamage(DamageArgs args)
		{
			var target = _aiBlackboard.GetVariableValue<IAiTarget>("AiTarget");
			if(args.DamageSource is null || args.DamageSource.Uid == CurrentContext.Uid) 
				return;

			if (target is { IsActive: true })
				return;
			
			if (!args.DamageSource.TryGetAiTarget(out _aiTarget))
				return;
			
			_targetSelector.AddTargetPriority(_aiTarget, args.Damage * 0.05f);
		}

		public override void OnDied()
		{
			if (_aiMovementController)
			{
				_aiMovementController.Disable();
			}
			
			DeathTask().Forget();
		}
		
		private async UniTaskVoid DeathTask()
		{
			await UniTask.Delay(5f.ToSec(), cancellationToken: destroyCancellationToken);
			await LMotion.Create(0f, 1f, 1f).Bind(SetDissolve).ToUniTask(destroyCancellationToken);
			DestroyBoss();
		}
		
		public void DestroyBoss()
		{
			CurrentContext.OnDestroyed(_entityRepository);
			Destroy(gameObject);
		}

		private void SetDissolve(float value)
		{
			foreach (var render in CurrentContext.Renderers)
			{
				foreach (var material in render.materials)
				{
					if(material.HasFloat(ShHash.DissolveSlider))
						material.SetFloat(ShHash.DissolveSlider, value);
				}
			}
		}

		public override void OnBossRoomOpened()
		{
			CurrentContext.OnStartFight();
			_aiBlackboard.SetVariableValue("InBossRoom", false);
		}

		public override void EnableControl()
		{
			_aiBlackboard.SetVariableValue("InDrag", false);
		}

		public override void DisableControl()
		{
			_aiBlackboard.SetVariableValue("InDrag", true);
		}
	}
}