using System.Collections;
using System.Threading;
using ControlFreak2;
using Core.CameraSystem;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.InputSystem;
using Cysharp.Threading.Tasks;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.PlayerTests
{
	public class PlayerTests : UiTest
	{
		[UnityTest]
		public IEnumerator RunningWhileChangeCharacters() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var entityRepository = scope.Container.Resolve<IEntityRepository>();

			DestroyAllLifeScene(scope.Container, false);

			await UniTask.Delay(100, cancellationToken: CancellationToken);

			FakeUserInput(CancellationToken).Forget();
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerFromId("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			playerSpawnService.PlayerCharacterAdapter.CurrentContext.OnDestroyed(entityRepository);
			Object.Destroy(context.gameObject);
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			await UniTask.Delay(2000, cancellationToken: CancellationToken);
		});



		[UnityTest]
		public IEnumerator ZiplineTest_DefaultMapTPV() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			DestroyAllLifeScene(scope.Container, false);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = new Vector3(26.58f, 32.5f, -40.7f);
			var ziplineInteraction = Object.FindFirstObjectByType<ZiplineInteraction>();
			ziplineInteraction.Use(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
			await UniTask.WaitWhile(() => ziplineInteraction.IsPlayerMoved, cancellationToken: CancellationToken);

			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator ZiplineTest_DefaultMapFPV() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var cameraService = scope.Container.Resolve<ICameraService>();
			cameraService.SetFirstPerson();

			DestroyAllLifeScene(scope.Container, false);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = new Vector3(26.58f, 32.5f, -40.7f);
			var ziplineInteraction = Object.FindFirstObjectByType<ZiplineInteraction>();
			ziplineInteraction.Use(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
			await UniTask.WaitWhile(() => ziplineInteraction.IsPlayerMoved, cancellationToken: CancellationToken);

			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});
	}
}