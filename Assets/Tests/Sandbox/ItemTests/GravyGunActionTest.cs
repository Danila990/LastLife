using System.Collections;
using System.Linq;
using Core.Actions.Impl;
using Core.Entity.Repository;
using Core.InputSystem;
using Core.Inventory.Items;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Tests.UITest;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.ItemTests
{
	public class GravyGunActionTest : UiTest
	{
		[UnityTest]
		public IEnumerator SpawnItem() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var repository = scope.Container.Resolve<IEntityRepository>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var maxUid = repository.EntityContext.Max(context => context.Uid);

			var gravyGun = FindFirstPlayerItem<GravyGun>(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
			var createItemAction = (CreateItemAction) gravyGun.ActionProvider.ActionControllers.FirstOrDefault(controller => controller.EntityAction is CreateItemAction)?.EntityAction;

			createItemAction.OnInputDown();

			var afterSpawn = repository.EntityContext.Max(context => context.Uid);
			Assert.Greater(afterSpawn, maxUid);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator DeleteItem() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var repository = scope.Container.Resolve<IEntityRepository>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var maxUid = repository.EntityContext.Max(context => context.Uid);

			var gravyGun = FindFirstPlayerItem<GravyGun>(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
			var createItemAction = (CreateItemAction) gravyGun.ActionProvider.ActionControllers.FirstOrDefault(controller => controller.EntityAction is CreateItemAction)?.EntityAction;

			createItemAction.OnInputDown();

			var afterSpawn = repository.EntityContext.Max(context => context.Uid);
			Assert.Greater(afterSpawn, maxUid);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});
	}
}