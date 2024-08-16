using System.Collections;
using System.Linq;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head.Bird;
using Core.Entity.InteractionLogic;
using Core.Entity.Repository;
using Core.Factory;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace Tests.Sandbox.EnemyTests
{
	public class HeadBirdTests : UiTest
	{
		[UnityTest]
		public IEnumerator DefaultCreateBird() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();

			await UniTask.Delay(2000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator IgnoreExplosionDamageInDefaultState() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();
			head.BehaviourTreeOwner.StopBehaviour();
			var overlapInteraction = scope.Container.Resolve<IOverlapInteractionService>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var characterContext = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			var healthBefore = head.CurrentContext.Health.CurrentHealth;
			CreateExplosion(scope, head, out var explosionVisitor, out var behaviour);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			Assert.AreEqual(healthBefore, head.CurrentContext.Health.CurrentHealth);

			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator IgnoreExplosionEntitiesDamage() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();
			head.BehaviourTreeOwner.StopBehaviour();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();


			var healthBefore = head.CurrentContext.Health.CurrentHealth;
			((ExplosionEntity) objectFactory.CreateObject("C4", GetPointOnCircle(head))).ExplosionVFX();
			((ExplosionEntity) objectFactory.CreateObject("C4", GetPointOnCircle(head))).ExplosionVFX();
			((ExplosionEntity) objectFactory.CreateObject("C4", GetPointOnCircle(head))).ExplosionVFX();

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			Assert.AreEqual(healthBefore, head.CurrentContext.Health.CurrentHealth);

			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		private static Vector3 GetPointOnCircle(AiHeadAdapter head)
		{
			var angle = 2.0f * Mathf.PI * Random.value;
			var pointOnCircle = new Vector3(8 * Mathf.Cos(angle), 8 * Mathf.Sin(angle));
			return head.CurrentContext.MainTransform.position + (Quaternion.LookRotation(Vector3.up) * pointOnCircle);
		}

		[UnityTest]
		public IEnumerator ExplosionDamageOpenMouth() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();
			head.BehaviourTreeOwner.StopBehaviour();
			head.BehaviourTreeOwner.behaviour.primeNode = head.BehaviourTreeOwner.behaviour.allNodes.Find(node => node.tag == "OpenMouth");
			head.BehaviourTreeOwner.StartBehaviour();

			await UniTask.Delay(3000, cancellationToken: CancellationToken);
			var healthBefore = head.CurrentContext.Health.CurrentHealth;
			CreateExplosion(scope, head, out var explosionVisitor, out var behaviour);

			await UniTask.Delay(3000, cancellationToken: CancellationToken);

			Assert.AreEqual(healthBefore - (behaviour.DamageMultiply * explosionVisitor.Args.Damage), head.CurrentContext.Health.CurrentHealth);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator ExplosionOutsideMouth() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();
			head.BehaviourTreeOwner.StopBehaviour();
			head.BehaviourTreeOwner.behaviour.primeNode = head.BehaviourTreeOwner.behaviour.allNodes.Find(node => node.tag == "OpenMouth");
			head.BehaviourTreeOwner.StartBehaviour();
			var healthBefore = head.CurrentContext.Health.CurrentHealth;
			await UniTask.Delay(3000, cancellationToken: CancellationToken);
			CreateExplosion(scope, head, out _, out _, new Vector3(5, 0, 1));
			CreateExplosion(scope, head, out _, out _, new Vector3(6, 0, 1));
			CreateExplosion(scope, head, out _, out _, new Vector3(0, 0, 10));
			//CreateExplosion(scope, head, out explosionVisitor, out behaviour, new Vector3(0, 0, -7));

			await UniTask.Delay(3500, cancellationToken: CancellationToken);

			Assert.AreEqual(healthBefore, head.CurrentContext.Health.CurrentHealth);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator SpawnEggHeadBirdTest() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();

			head.BehaviourTreeOwner.StopBehaviour();
			head.BehaviourTreeOwner.behaviour.primeNode = head.BehaviourTreeOwner.behaviour.allNodes.Find(node => node.tag == "SpawnEgg");
			head.BehaviourTreeOwner.StartBehaviour();
			var countBefore = scope.Container.Resolve<IEntityRepository>().EntityContext.Count();
			await UniTask.Delay(7000, cancellationToken: CancellationToken);
			var countAfter = scope.Container.Resolve<IEntityRepository>().EntityContext.Count();

			Assert.Greater(countAfter, countBefore);

		});

		[UnityTest]
		public IEnumerator SpawnEggInterruptionHeadBirdTest() => UniTask.ToCoroutine(async () =>
		{
			var (scope, strategyFactory, head) = await InitAndCreate();
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var defaultStart = head.BehaviourTreeOwner.behaviour.primeNode;
			head.BehaviourTreeOwner.StopBehaviour();
			head.BehaviourTreeOwner.behaviour.primeNode = head.BehaviourTreeOwner.behaviour.allNodes.Find(node => node.tag == "SpawnEgg");
			head.BehaviourTreeOwner.StartBehaviour();
			var countBefore = scope.Container.Resolve<IEntityRepository>().EntityContext.Count();

			var variable = head.AiBlackboard.GetVariable<bool>("Interrupted");
			var beforeExp = variable.value;
			var healthBefore = head.CurrentContext.Health.CurrentHealth;

			Assert.False(beforeExp);
			await UniTask.Delay(3000, cancellationToken: CancellationToken);
			CreateExplosion(scope, head, out var explosionVisitor, out var behaviour);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var afterExp = variable.value;
			Assert.True(afterExp);
			Assert.Less(head.CurrentContext.Health.CurrentHealth, healthBefore);
			var countAfter = scope.Container.Resolve<IEntityRepository>().EntityContext.Count();
			Assert.AreEqual(countBefore, countAfter);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
		});


		private async UniTask<(LifetimeScope scope, IAdapterStrategyFactory strategyFactory, AiHeadAdapter head)> InitAndCreate()
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			DestroyAllLifeScene(scope.Container, true);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = strategyFactory.CreateAiAdapter<AiHeadAdapter>("HeadBird", Vector3.zero, Quaternion.LookRotation(Vector3.back));
			return (scope, strategyFactory, head);
		}

		private static void CreateExplosion(
			LifetimeScope scope,
			AiHeadAdapter head,
			out InternalExplosionVisitor explosionVisitor,
			out DamageBehaviour behaviour,
			Vector3 offset = default)
		{
			var overlapInteraction = scope.Container.Resolve<IOverlapInteractionService>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var characterContext = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			explosionVisitor = new InternalExplosionVisitor()
			{
				Args = new SerializedDamageArgs()
				{
					Damage = 1000,
					DamageType = DamageType.Explosion
				}
			};

			behaviour = ((HeadBird) head.CurrentContext).HeartDamagable.BehaviourData.GetBehaviour(DamageType.Explosion);


			explosionVisitor.SetOwner(characterContext);
			overlapInteraction.OverlapSphere(explosionVisitor, head.CurrentContext.LookAtTransform.position + offset, 15f);
		}
	}
}