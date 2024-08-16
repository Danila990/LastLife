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
using MiniGames.Impl.SpinnerMiniGame;
using SharedUtils;
using Tests.Mock;
using Tests.UITest;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Extensions;

namespace Tests.Market.Idler
{
    public class OilDerrekMinigamesTests : UiTest
    {
        protected override void OverrideRegistration()
        {
            InstallerSubstitution.Substitute<ISaveSystemService, MockSaveSystemService>();
        }
        
        [UnityTest]
        public IEnumerator OilMiniGame() => UniTask.ToCoroutine(async () =>
        {
            PlayerPrefs.DeleteAll();
            var scope = await InitMarket(CancellationToken);
            await UniTask.Delay(1000, cancellationToken: CancellationToken);
            var oilRigContext = Object.FindFirstObjectByType<OilRigContext>();
            var refinerFactory = Object.FindFirstObjectByType<RefinerFactoryContext>();
            
            var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
            var entityRepository = scope.Container.Resolve<IEntityRepository>();
            var boostRefiner = scope.Container.Resolve<IRefinerBoostService>();
            
            var barrels = entityRepository.EntityContext.Where(x => x is FuelBarrelContext).ToList();
            foreach (var b in barrels)
            {
                b.OnDestroyed(entityRepository);
                Object.Destroy(b.gameObject);
            }
            playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = oilRigContext.transform.position + -oilRigContext.transform.right * 4;
            await UniTask.Delay(500, cancellationToken: CancellationToken);
            var minigame = oilRigContext.GetComponent<InteractionMiniGameStarter>();
            minigame.Interaction.Used.Execute(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
            await UniTask.Delay(100, cancellationToken: CancellationToken);
            var spinUi = Object.FindFirstObjectByType<SpinnerUi>();
            var i = 0;
            UniTask.Void(async () =>
            {
                while (spinUi.Value.Value<60)
                {
                    await UniTask.NextFrame(CancellationToken);
                    i += 30;
                    var ang = i / 360f * Mathf.PI * 2f;
                    var x = Mathf.Sin(ang) * 10f;
                    var y = Mathf.Cos(ang) * 10f;
                    spinUi.OnDrag(new Vector2(x+spinUi.Center.x,y+spinUi.Center.y));
                }
            });
            await UniTask.WaitUntilValueChanged(entityRepository.EntityContext, contexts => contexts.Count(x => x is FuelBarrelContext));
            var barrel = Object.FindFirstObjectByType<FuelBarrelContext>().GetComponent<CarriedItemPickUpInteraction>();
            await oilRigContext.ConveyorObject.OnComplete.ToUniTask(true,cancellationToken: CancellationToken);
            barrel.Use(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
            await UniTask.Delay(1000, cancellationToken: CancellationToken);
            playerSpawnService.PlayerCharacterAdapter.Rigidbody.position =
                refinerFactory.RefinerObjects[0].transform.position-oilRigContext.transform.right;
            var canisters = Object.FindObjectsByType<FuelKanisterContext>(FindObjectsSortMode.None).Count();
            await UniTask.Delay(1000, cancellationToken: CancellationToken);
            boostRefiner.Boost(refinerFactory.RefinerObjects[0].Id, 0.01f, out var timer);
            await timer.OnEnd.ToUniTask(true, cancellationToken: CancellationToken);
            await refinerFactory.ProducerObject.ConveyorObject.OnComplete.ToUniTask(true,
                cancellationToken: CancellationToken);
            await UniTask.Delay(100, cancellationToken: CancellationToken);
            canisters = Object.FindObjectsByType<FuelKanisterContext>(FindObjectsSortMode.None).Count() - canisters;
            var list = Object.FindObjectsByType<FuelKanisterContext>(FindObjectsSortMode.None);
            foreach (var can in list)
            {
                can.GetComponent<CarriedItemPickUpInteraction>().Use(playerSpawnService.PlayerCharacterAdapter.CurrentContext);
                await UniTask.Delay(100, cancellationToken: CancellationToken);
            }
            await UniTask.Delay(100, cancellationToken: CancellationToken);
            Assert.IsTrue(canisters>0);
        });
    }
}