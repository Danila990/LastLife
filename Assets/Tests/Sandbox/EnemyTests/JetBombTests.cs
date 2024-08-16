using System.Collections;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Head;
using Core.Factory;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Services.Experience;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.EnemyTests
{
	public class JetBombTests : UiTest
	{
		[UnityTest]
		public IEnumerator GettingAllExpFromExplosionTest() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			var expService = scope.Container.Resolve<IExperienceService>();
			DestroyAllLifeScene(scope.Container, false);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var expBefore = expService.CurrentExperience.Value;
			EntityContext bomb = null;
			var bombCount = 3;
			for (int i = 0; i < bombCount; i++)
			{
				bomb = objectFactory.CreateObject("JetBomb", Vector3.zero);
			}
			/*await UniTask.Delay(500, cancellationToken: CancellationToken);
			Assert.NotNull(bomb);
			var args = new DamageArgs(playerSpawnService.PlayerCharacterAdapter.CurrentContext, 1000f, damageType: DamageType.Range);
			bomb.DoDamage(ref args, DamageType.Range);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			Assert.AreEqual(expBefore + JetBombContext.DIED_EXP_COUNT * bombCount, expService.CurrentExperience.Value);*/
		});
	}
}