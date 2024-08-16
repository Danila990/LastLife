using System.Collections;
using System.Linq;
using Core.Factory;
using Core.Factory.DataObjects;
using Core.InputSystem;
using Core.Loot;
using Core.Services;
using Cysharp.Threading.Tasks;
using Tests.UITest;
using Ticket;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.LootTests
{
	public class LootTest : UiTest
	{
		[UnityTest]
		public IEnumerator TicketTest() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var lootFactory = scope.Container.Resolve<ILootFactory>();
			var factoryData = scope.Container.Resolve<IFactoryData>();
			var ticketService = scope.Container.Resolve<ITicketService>();
			var storageService = scope.Container.Resolve<IItemStorage>();
			var unlockService = scope.Container.Resolve<IItemUnlockService>();

			foreach (var characterObjectData in storageService.Characters.Values)
			{
				unlockService.UnlockItem(characterObjectData);
			}

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			var ticketCount = ticketService.CurrentTicketsCount;
			var ticketsLootsData = factoryData.Objects.ToList().FindAll(x => x is {Type: EntityType.Loot, Object: TicketLootEntity});
			playerSpawnService.CreatePlayerFromId("AWPMan");

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var ticketLoot in ticketsLootsData)
			{
				lootFactory.CreateObject(ticketLoot.Key, context.MainTransform.position);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
				NUnit.Framework.Assert.Greater(ticketService.CurrentTicketsCount, ticketCount);
			}
		});

		[UnityTest]
		public IEnumerator ItemsTest() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var lootFactory = scope.Container.Resolve<ILootFactory>();
			var factoryData = scope.Container.Resolve<IFactoryData>();
			var storageService = scope.Container.Resolve<IItemStorage>();
			var unlockService = scope.Container.Resolve<IItemUnlockService>();

			foreach (var characterObjectData in storageService.Characters.Values)
			{
				unlockService.UnlockItem(characterObjectData);
			}

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			var itemLootsData =
				factoryData.Objects.ToList()
					.FindAll(x => x is {Type: EntityType.Loot, Object: ItemLootEntity});
			playerSpawnService.CreatePlayerFromId("AWPMan");

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var itemLoot in itemLootsData)
			{
				lootFactory.CreateObject(itemLoot.Key, context.MainTransform.position);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
				var item = context.Inventory.InventoryItems
					.First(x => x.Id == ((ItemLootEntity) itemLoot.Object).ObjectToGiveTest.ObjectData.Id);
				Assert.IsNotNull(item.InventoryObject);
			}
		});
	}
}