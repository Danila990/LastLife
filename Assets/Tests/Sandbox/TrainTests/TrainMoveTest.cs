using System.Collections;
using System.Linq;
using Core.Entity.InteractionLogic.Interactions;
using Core.InputSystem;
using Cysharp.Threading.Tasks;
using Dialogue.Services;
using Dialogue.Ui;
using Tests.UITest;
using Tests.UITest.Utils;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.TrainTests
{
    public class TrainMoveTest : UiTest
    {
        [UnityTest]
        public IEnumerator TestMove() => UniTask.ToCoroutine(async () =>
        {
            var scope = await InitSandbox(CancellationToken);
            scope.Container.Resolve<ITicketService>().OnPurchaseTickets(5);

            var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
            var pTransform = playerSpawnService.PlayerCharacterAdapter.transform;
            var mapName = SceneManager.GetActiveScene().name;
            await UniTask.Delay(2500, cancellationToken: CancellationToken);
            await TeleportToPos(playerSpawnService,new Vector3(4.30460262f,3.66905618f,-95.5300674f));
            await UniTask.Delay(100, cancellationToken: CancellationToken);
            var ui = Object.FindObjectsByType<LoadLevelInteraction>(FindObjectsSortMode.None).OrderBy(x=>(x.transform.position-pTransform.position).magnitude).First();
            ui.Load();
            while (mapName.Equals(SceneManager.GetActiveScene().name))
            {
                await UniTask.NextFrame(CancellationToken);
            }
            
            await UniTask.Delay(2500, cancellationToken: CancellationToken);
            
            scope = DiUtils.SceneScope;
            playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
            pTransform = playerSpawnService.PlayerCharacterAdapter.transform;
            mapName = SceneManager.GetActiveScene().name;
            await UniTask.Delay(100, cancellationToken: CancellationToken);
            await TeleportToPos(playerSpawnService,new Vector3(-4.78217602f,-1.69830549f,35.925354f));
            await UniTask.Delay(100, cancellationToken: CancellationToken);
            ui = Object.FindObjectsByType<LoadLevelInteraction>(FindObjectsSortMode.None).OrderBy(x=>(x.transform.position-pTransform.position).magnitude).First();
            ui.Load();
            while (mapName.Equals(SceneManager.GetActiveScene().name))
            {
                await UniTask.NextFrame(CancellationToken);
            }
            
            await UniTask.Delay(1000, cancellationToken: CancellationToken);
            
            Assert.IsTrue(true);
        });
        
        private async UniTask TeleportToPos(IPlayerSpawnService transform, Vector3 pos)
        {
            var player = transform.PlayerCharacterAdapter;
            var target = pos;
            for (var i = 0; i < 100; i++)
            {
                player.transform.position = target;
                await UniTask.NextFrame();
                if ((player.transform.position - target).sqrMagnitude < 1) return;
            }
        }
    }
}