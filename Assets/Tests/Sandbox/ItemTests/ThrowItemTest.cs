using System.Collections;
using System.Linq;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Repository;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Core.Services;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.ItemTests
{
	public class ThrowItemTest : UiTest
	{
		[UnityTest] [NotNull]
		public IEnumerator ThrowGrenadeItemAndKillAfter1Sec() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var repository = scope.Container.Resolve<IEntityRepository>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			var id = "GrenadeItem";
			context.Inventory.AddItem(itemStorage.InventoryItems[id], 2);

			var item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);

			var grenade = item.ItemContext as ThrowableWeaponContext;
			Assert.IsNotNull(grenade);

			grenade.WeaponAdapter.Throw();
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			KillContext(context, grenade);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			playerSpawnService.CreatePlayerFromId("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);
			Debug.Log($"{item.ItemContext.ItemId} {item.ItemContext.CurrentQuantity}");
		});

		[UnityTest] [NotNull]
		public IEnumerator ThrowGrenadeItemAndKillAfter100Sec() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var repository = scope.Container.Resolve<IEntityRepository>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			var id = "GrenadeItem";
			context.Inventory.AddItem(itemStorage.InventoryItems[id], 2);

			var item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);

			var grenade = item.ItemContext as ThrowableWeaponContext;
			Assert.IsNotNull(grenade);

			grenade.WeaponAdapter.Throw();
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			KillContext(context, grenade);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			playerSpawnService.CreatePlayerFromId("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);
			Debug.Log($"{item.ItemContext.ItemId} {item.ItemContext.CurrentQuantity}");
		});

		[UnityTest] [NotNull]
		public IEnumerator ThrowGrenadeItemAndKill() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var repository = scope.Container.Resolve<IEntityRepository>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			var id = "GrenadeItem";
			context.Inventory.AddItem(itemStorage.InventoryItems[id], 2);

			var item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);

			var grenade = item.ItemContext as ThrowableWeaponContext;
			Assert.IsNotNull(grenade);

			grenade.WeaponAdapter.Throw();
			KillContext(context, grenade);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			playerSpawnService.CreatePlayerFromId("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.NotNull(item.ItemContext);
			Debug.Log($"{item.ItemContext.ItemId} {item.ItemContext.CurrentQuantity}");
		});

		[UnityTest] [NotNull]
		public IEnumerator ThrowGrenadeItemAndKillAfter10Sec() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var repository = scope.Container.Resolve<IEntityRepository>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			var id = "GrenadeItem";
			context.Inventory.AddItem(itemStorage.InventoryItems[id], 2);

			var item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);

			var grenade = item.ItemContext as ThrowableWeaponContext;
			Assert.IsNotNull(grenade);

			grenade.WeaponAdapter.Throw();
			await UniTask.Delay(10000, cancellationToken: CancellationToken);

			KillContext(context, grenade);

			await UniTask.Delay(500, cancellationToken: CancellationToken);
			playerSpawnService.CreatePlayerFromId("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			item = context.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext.ItemId == id);
			Assert.IsNotNull(item);
			Debug.Log($"{item.ItemContext.ItemId} {item.ItemContext.CurrentQuantity}");

		});

		private void KillContext(CharacterContext context, EntityContext from)
		{
			var dmg = new DamageArgs(from, context.Health.CurrentHealth);
			context.Health.DoDamage(ref dmg);


		}
	}
}