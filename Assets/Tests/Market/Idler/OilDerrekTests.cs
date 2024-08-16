using System.Collections;
using System.Linq;
using Core.Carry;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.InputSystem;
using Core.Services.SaveSystem;
using Cysharp.Threading.Tasks;
using Market.OilRig;
using MiniGames;
using NUnit.Framework;
using SharedUtils;
using Tests.Mock;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Extensions;

namespace Tests.Market.Idler
{
	public class OilDerrekTests : UiTest
	{
		protected override void OverrideRegistration()
		{
			InstallerSubstitution.Substitute<IMiniGameService, MockMiniGameService>();
			InstallerSubstitution.Substitute<ISaveSystemService, MockSaveSystemService>();
		}

		[UnityTest]
		public IEnumerator CreateOilBarrelTestAfterWinMiniGame() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitMarket(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var oilRigContext = Object.FindFirstObjectByType<OilRigContext>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var entityRepository = scope.Container.Resolve<IEntityRepository>();
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = oilRigContext.transform.position + -oilRigContext.transform.right * 4;
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			UniTask.Void(async data =>
			{
				await UniTask.Delay(500, cancellationToken: CancellationToken);
				var (spawnService, rigContext) = data;
				rigContext.GetComponent<GenericInteraction>().Used.Execute(spawnService.PlayerCharacterAdapter.CurrentContext);
			}, (playerSpawnService, oilRigContext));

			await UniTask.WaitUntilValueChanged(entityRepository.EntityContext, contexts => contexts.Count).Timeout(2f.ToSec());
			Assert.IsInstanceOf<CarriedContext>(entityRepository.EntityContext.Last());
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});
		

	}

}