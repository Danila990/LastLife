using System.Collections;
using System.Linq;
using Core.Actions;
using Core.Actions.Impl;
using Core.Boosts;
using Core.Boosts.Impl;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic.Interactions;
using Core.Factory;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Core.Services;
using Cysharp.Threading.Tasks;
using GameStateMachine.States.Impl.Project;
using MessagePipe;
using Tests.UITest;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Boost
{
	public class BoostApplyTest : UiTest
	{
		[UnityTest]
		public IEnumerator SaveBoostBetweenScene() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var loaderService = scope.Container.Resolve<ISceneLoaderService>();
			var player = playerSpawnService.PlayerCharacterAdapter;

			var boostArg = new BoostArgs
			{
				Category = BoostCategory.Special,
				Type = BoostTypes.HP,
				Duration = 100,
				Value = 100
			};
			
			player.BoostsInventory.Add(boostArg, 1);
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			player.BoostProvider.ApplyBoost(BoostTypes.HP);
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			Assert.IsTrue(player.BoostProvider.ActiveBoosts.TryGetValue(BoostTypes.HP, out _));
			Assert.IsFalse(player.BoostProvider.ActiveBoosts.TryGetValue(BoostTypes.DAMAGE, out _));
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			
			
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			scope = await LoadThroughGameScene(SceneLoaderService.MARKET, CancellationToken);
			playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			player = playerSpawnService.PlayerCharacterAdapter;
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			Debug.Log(scope.gameObject.scene.name);
			Debug.LogWarning("Test on Market");
			Assert.IsTrue(player.BoostProvider.ActiveBoosts.TryGetValue(BoostTypes.HP, out _), "Check HP after load Market Failed");
			Assert.IsFalse(player.BoostProvider.ActiveBoosts.TryGetValue(BoostTypes.DAMAGE, out _), "Check DMG after load Market Failed");
			await UniTask.Delay(1500, cancellationToken: CancellationToken);
			
			
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			scope = await LoadThroughGameScene(SceneLoaderService.SANDBOX, CancellationToken);
			playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			player = playerSpawnService.PlayerCharacterAdapter;
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			Assert.IsTrue(player.BoostProvider.ActiveBoosts.TryGetValue(BoostTypes.HP, out _));
			Assert.IsFalse(player.BoostProvider.ActiveBoosts.TryGetValue(BoostTypes.DAMAGE, out _));
			await UniTask.Delay(1500, cancellationToken: CancellationToken);
		});
		
		
		[UnityTest]
		public IEnumerator JumpBoost() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitMarket(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var player = playerSpawnService.PlayerCharacterAdapter;

			await TeleportToCenter(player.Rigidbody);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var delta = await JumpTest(player);

			Assert.IsTrue(delta > 5);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator SpeedBoost() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitMarket(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var player = playerSpawnService.PlayerCharacterAdapter;

			await TeleportToCenter(player.Rigidbody);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var delta = await RunTest(player);

			Assert.IsTrue(delta > 5);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator HpBoost() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitMarket(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var player = playerSpawnService.PlayerCharacterAdapter;

			await TeleportToCenter(player.Rigidbody);
			var boostArg = new BoostArgs
			{
				Category = BoostCategory.Special,
				Type = BoostTypes.HP,
				Duration = 10,
				Value = 100
			};
			player.BoostsInventory.Add(boostArg, 1);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			player.BoostProvider.ApplyBoost(BoostTypes.HP);
			var args = new DamageArgs(null, player.CurrentContext.Health.CurrentHealth / 2);
			player.CurrentContext.Health.DoDamage(ref args);
			var delta = player.CurrentContext.Health.CurrentHealth;
			await UniTask.Delay(3000, cancellationToken: CancellationToken);
			delta = player.CurrentContext.Health.CurrentHealth - delta;
			Assert.IsTrue(delta > 5);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator DamageBoost() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var factory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var player = playerSpawnService.PlayerCharacterAdapter;
			var gun = FindFirstPlayerItem<SimpleProjectileWeaponContext>(player.CurrentContext);
			player.CurrentContext.Inventory.SelectItem(gun);
			player.AimController.SetAimState(AimState.Aim);
			var holder = new GameObject("TestHolder").transform;
			var origin = gun.GetOrigin();
			origin.SetParent(holder);
			var shootAction = gun.ActionProvider.ActionControllers.First(x => x.ActionKey.Equals(ActionKey.MainAction)).EntityAction as ShootingAction;
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			await TeleportToCenter(player.Rigidbody);
			var delta1 = await TestDummyGun(factory, shootAction, holder);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var boostArg = new BoostArgs
			{
				Category = BoostCategory.Valued,
				Type = BoostTypes.DAMAGE,
				Duration = 10,
				Value = 100
			};
			player.BoostsInventory.Add(boostArg, 1);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			player.BoostProvider.ApplyBoost(BoostTypes.DAMAGE);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var delta2 = await TestDummyGun(factory, shootAction, holder);

			Assert.IsTrue((delta2 - delta1) > 5);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		private async UniTask<float> TestDummyGun(IAdapterStrategyFactory factory, ShootingAction shootAction, Transform holder)
		{
			var dummy = CreateDummy(factory, new Vector3(-10f, 0, -8));
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			shootAction.OnInputDown();
			var dummyHealth = dummy.CurrentContext.Health.CurrentHealth;
			while (dummyHealth.Equals(dummy.CurrentContext.Health.CurrentHealth))
			{
				holder.position = new Vector3(-10, 0.5f, -9);
				holder.rotation = Quaternion.Euler(0, 0, 0);
				await UniTask.NextFrame(CancellationToken);
			}
			shootAction.OnInputUp();
			dummy.transform.position = new Vector3(100, 100, 100);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			return dummyHealth - dummy.CurrentContext.Health.CurrentHealth;
		}

		private async UniTask<float> RunTest(PlayerCharacterAdapter player)
		{
			var boostArg = new BoostArgs
			{
				Category = BoostCategory.Valued,
				Type = BoostTypes.SPEED_UP,
				Duration = 10,
				Value = 100
			};
			player.BoostsInventory.Add(boostArg, 1);
			await TeleportToCenter(player.Rigidbody);
			await FakeUserInput(player.GetCancellationTokenOnDestroy(), 2);
			var delta = (new Vector3(-10, 0, -10) - player.Rigidbody.position).sqrMagnitude;
			await TeleportToCenter(player.Rigidbody);
			player.BoostProvider.ApplyBoost(BoostTypes.SPEED_UP);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			await FakeUserInput(player.GetCancellationTokenOnDestroy(), 2);
			var delta2 = (new Vector3(-10, 0, -10) - player.Rigidbody.position).sqrMagnitude;
			var boost = delta2 - delta;
			return boost;
		}

		private async UniTask<float> JumpTest(PlayerCharacterAdapter player)
		{
			var boostArg = new BoostArgs
			{
				Category = BoostCategory.Valued,
				Type = BoostTypes.JUMP_UP,
				Duration = 10,
				Value = 100
			};
			player.BoostsInventory.Add(boostArg, 1);

			//jump withouth boost
			player.MovementStateMachine.ChangeState(player.MovementStateMachine.JumpingState);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var playerJump = player.transform.position.y;
			await UniTask.Delay(2500, cancellationToken: CancellationToken);

			//jump with boost
			player.BoostProvider.ApplyBoost(BoostTypes.JUMP_UP);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			player.MovementStateMachine.ChangeState(player.MovementStateMachine.JumpingState);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var newPos = player.transform.position.y;
			var delta = newPos - playerJump;
			return delta;
		}

		private async UniTask TeleportToCenter(Rigidbody transform)
		{
			var target = new Vector3(-10, 0.1f, -10);
			for (var i = 0; i < 100; i++)
			{
				transform.position = target;
				await UniTask.NextFrame();
				if ((transform.position - target).sqrMagnitude < 1) return;
			}
		}
	}
}