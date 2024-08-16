using System.Collections;
using Core.Entity.Characters.Adapters;
using Core.Factory;
using Cysharp.Threading.Tasks;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.EnemyTests
{
	public class JetHeadTests : UiTest
	{
		[UnityTest]
		public IEnumerator CreateJetHead() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			DestroyAllLifeScene(scope.Container, false);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = strategyFactory.CreateAiAdapter<AiHeadAdapter>("JetHead", Vector3.zero, Quaternion.LookRotation(Vector3.back));
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});
	}
}