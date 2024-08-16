using System.Collections;
using System.Linq;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Factory;
using Core.Services;
using Cysharp.Threading.Tasks;
using Db.ObjectData;
using NUnit.Framework;
using SharedUtils;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.AiTests
{
	public class AiCharacterTests : UiTest
	{
		protected override void OverrideRegistration()
		{

		}

		[UnityTest]
		public IEnumerator DeathTaskTest() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var storage = scope.Container.Resolve<IItemStorage>();
			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();

			foreach (var characterData in storage.Characters.Values.Where(data => data.AiType == AiType.Character))
			{
				var aiAdapter = strategyFactory.CreateAiAdapter<AiCharacterAdapter>(characterData.AiAdapterId, Vector3.zero, Quaternion.identity);
				Assert.NotNull(aiAdapter, "aiAdapter != null");
				aiAdapter.DeathTask().Forget();
				await UniTask.Delay((aiAdapter.DissolveDuration + 0.5f).ToSec(), cancellationToken: CancellationToken);
				UnityEngine.Assertions.Assert.IsNull(aiAdapter);

				await UniTask.Delay(0.1f.ToSec(), cancellationToken: CancellationToken);
			}
		});

		[UnityTest]
		public IEnumerator DeathTaskWithDoubleDeathTask() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var storage = scope.Container.Resolve<IItemStorage>();
			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();

			foreach (var characterData in storage.Characters.Values.Where(data => data.AiType == AiType.Character).Take(3))
			{
				var aiAdapter = strategyFactory.CreateAiAdapter<AiCharacterAdapter>(characterData.AiAdapterId, Vector3.zero, Quaternion.identity);
				Assert.NotNull(aiAdapter, "aiAdapter != null");
				aiAdapter.DeathTask().Forget();
				await UniTask.Delay((aiAdapter.DissolveDuration / 2f).ToSec(), cancellationToken: CancellationToken);
				aiAdapter.DeathTask().Forget();
				aiAdapter.DeathTask().Forget();
				await UniTask.Delay((aiAdapter.DissolveDuration + 0.5f).ToSec(), cancellationToken: CancellationToken);

				await UniTask.Delay(0.1f.ToSec(), cancellationToken: CancellationToken);
			}
		});

		[UnityTest]
		public IEnumerator DeathTaskWithDestroyTest() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var storage = scope.Container.Resolve<IItemStorage>();
			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var entityRepository = scope.Container.Resolve<IEntityRepository>();

			foreach (var characterData in storage.Characters.Values.Where(data => data.AiType == AiType.Character).Take(3))
			{
				var aiAdapter = strategyFactory.CreateAiAdapter<AiCharacterAdapter>(characterData.AiAdapterId, Vector3.zero, Quaternion.identity);
				var aiContext = aiAdapter.CurrentContext;
				Assert.NotNull(aiAdapter, "aiAdapter != null");
				aiAdapter.DeathTask().Forget();

				await UniTask.Delay((aiAdapter.DissolveDuration / 2f).ToSec(), cancellationToken: CancellationToken);
				aiAdapter.CurrentContext.OnDestroyed(entityRepository);
				Object.Destroy(aiAdapter.gameObject);
				await UniTask.Delay(0.1f.ToSec(), cancellationToken: CancellationToken);

				UnityEngine.Assertions.Assert.IsNull(aiContext);
				await UniTask.Delay(0.1f.ToSec(), cancellationToken: CancellationToken);
			}
		});
	}
}