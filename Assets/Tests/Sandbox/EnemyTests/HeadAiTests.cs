using System.Collections;
using System.Linq;
using Core.Entity.Ai.AiItem.Data;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.Entity.Repository;
using Core.Factory;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Cysharp.Threading.Tasks;
using Db.ObjectData.Impl;
using NUnit.Framework;
using Tests.UITest;
using Ui.Sandbox.CharacterMenu;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Tests.Sandbox.EnemyTests
{
	public class HeadAiTests : UiTest
	{
		protected override void OverrideRegistration()
		{

		}

		[UnityTest]
		public IEnumerator HeadExplodeTankerTest() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			DestroyAllLifeScene(scope.Container, true);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = (HeadContext) objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));
			var backTankDamagable = head.GetComponentInChildren<BackTankDamagable>();
			var explosionEntity = ((ExplosionEntity) objectFactory.CreateObject("C4", backTankDamagable.transform.position + new Vector3(0, 2.5f, 0)));
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			explosionEntity.ExplosionVFX();
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			Assert.True(backTankDamagable.IsBroken);
		});

		[UnityTest]
		public IEnumerator TongueEating_AiCharacterAdapter() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();

			DestroyAllLifeScene(scope.Container, true);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);
			var pistolMan = strategyFactory.CreateAiAdapter<AiCharacterAdapter>("PistolMan", new Vector3(0, 0, -10));
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			pistolMan.CurrentContext.TryGetAiTarget(out var aiTarget);
			aiTongue.Use(aiTarget);
			await UniTask.WaitWhile(() => aiTongue.InUse);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator TongueEating_PlayerCharacterAdapter() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			DestroyAllLifeScene(scope.Container);


			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			playerSpawnService.PlayerCharacterAdapter.CurrentContext.TryGetAiTarget(out var aiTarget);
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = new Vector3(0, 0, -10);
			await UniTask.Delay(300, cancellationToken: CancellationToken);
			aiTongue.Use(aiTarget);
			await UniTask.WaitWhile(() => aiTongue.InUse);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator TongueEating_ThroughWalls([ValueSource(nameof(ThroughWalls_TestCases))] DoublePosition positions) => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			DestroyAllLifeScene(scope.Container);


			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", positions.First, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			playerSpawnService.PlayerCharacterAdapter.CurrentContext.TryGetAiTarget(out var aiTarget);
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = positions.Second;

			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			aiTongue.Use(aiTarget);
			await UniTask.WaitWhile(() => aiTongue.InUse);
			Assert.True(!playerSpawnService.PlayerCharacterAdapter.CurrentContext.Health.IsDeath);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		private static IEnumerable ThroughWalls_TestCases()
		{
			yield return new DoublePosition
			{
				First = new Vector3(-30.7f, 0.6f, -32.26f),
				Second = new Vector3(-30.7f, 13.6f, -32.26f),
				Name = "Roof"
			};

			yield return new DoublePosition
			{
				First = new Vector3(0.07f, -0.005f, 30f),
				Second = new Vector3(1.47f, -3.83f, 30f),
				Name = "Tunnel"
			};
		}

		public struct DoublePosition
		{
			public Vector3 First;
			public Vector3 Second;
			public string Name;

			public override string ToString()
			{
				return Name;
			}
		}

		[UnityTest]
		public IEnumerator TongueEating_OutOfRange() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();

			DestroyAllLifeScene(scope.Container);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);
			var pistolMan = strategyFactory.CreateAiAdapter<AiCharacterAdapter>("PistolMan", new Vector3(0, 0, -40));
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			pistolMan.CurrentContext.TryGetAiTarget(out var aiTarget);
			aiTongue.Use(aiTarget);
			await UniTask.WaitWhile(() => aiTongue.InUse);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator TongueEating_AiCharacterAdapter_WithDestroy([ValueSource(nameof(TestCases))] int time) => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var objectFactory = scope.Container.Resolve<IObjectFactory>();

			DestroyAllLifeScene(scope.Container, true);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);
			var pistolMan = strategyFactory.CreateAiAdapter<AiCharacterAdapter>("PistolMan", new Vector3(0, 0, -10));
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			pistolMan.CurrentContext.TryGetAiTarget(out var aiTarget);
			aiTongue.Use(aiTarget);
			await UniTask.Delay(time, cancellationToken: CancellationToken);

			if (!pistolMan)
			{
				Debug.LogError("Already destroyed");
				return;
			}
			var entityRepository = scope.Container.Resolve<IEntityRepository>();
			pistolMan.CurrentContext.OnDestroyed(entityRepository);
			Object.Destroy(pistolMan.gameObject);
			Debug.Log("OnDestroyed");


			await UniTask.Delay(2000, cancellationToken: CancellationToken);
		});

		private static IEnumerable TestCases()
		{
			yield return 200;
			yield return 1000;
			yield return 2600;
		}

		[UnityTest]
		public IEnumerator TongueEating_PlayerCharacterAdapter_WithCharacterSwitch() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();

			DestroyAllLifeScene(scope.Container);


			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			playerSpawnService.PlayerCharacterAdapter.CurrentContext.TryGetAiTarget(out var aiTarget);
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = new Vector3(0, 0, -10);
			await UniTask.Delay(300, cancellationToken: CancellationToken);
			aiTongue.Use(aiTarget);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerFromId("AWPMan");
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
		});

		[UnityTest]
		public IEnumerator TongueEating_PlayerCharacterAdapter_WithInventoryItems() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);

			var objectFactory = scope.Container.Resolve<IObjectFactory>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var characterMenuController = scope.Container.Resolve<CharacterMenuController>();

			DestroyAllLifeScene(scope.Container);

			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var head = objectFactory.CreateObject("HeadBase", Vector3.zero, Quaternion.LookRotation(Vector3.back));

			var tongueAiItemData = AssetDatabase.LoadAssetAtPath<TongueAiItemData>("Assets/Settings/Data/Ai/AiItems/TongueAiItemData.asset");
			var aiTongue = tongueAiItemData.CreateAiItem(head);
			scope.Container.Inject(aiTongue);
			await UniTask.Delay(300, cancellationToken: CancellationToken);

			playerSpawnService.PlayerCharacterAdapter.CurrentContext.TryGetAiTarget(out var aiTarget);
			playerSpawnService.PlayerCharacterAdapter.Rigidbody.position = new Vector3(0, 0, -10);
			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			var c4Item = AssetDatabase.LoadAssetAtPath<InventoryObjectDataSo>("Assets/Settings/Data/ObjectsData/InventoryItems/Throwable/C4Data.asset");
			var grenadeItem = AssetDatabase.LoadAssetAtPath<InventoryObjectDataSo>("Assets/Settings/Data/ObjectsData/InventoryItems/Throwable/GrenadeData.asset");
			var mineItem = AssetDatabase.LoadAssetAtPath<InventoryObjectDataSo>("Assets/Settings/Data/ObjectsData/InventoryItems/Throwable/LaserMineData.asset");

			context.Inventory.AddItem(c4Item.Model, 1);
			context.Inventory.AddItem(grenadeItem.Model, 1);
			context.Inventory.AddItem(mineItem.Model, 1);

			var toSave = context.Inventory
				.InventoryItems
				.Where(context =>
					context.InventoryObject.Savable &&
					(context.ItemContext.HasQuantity &&
					 context.ItemContext.CurrentQuantity.Value > 0))
				.Select(pair => (pair.ItemContext.CurrentQuantity.Value, pair.InventoryObject))
				.ToArray();

			await UniTask.Delay(300, cancellationToken: CancellationToken);
			aiTongue.Use(aiTarget);

			await UniTask.Delay(8000, cancellationToken: CancellationToken);
			characterMenuController.ClickPlayForTest();
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;


			foreach (var (count, inventoryObject) in toSave)
			{
				var item = context.Inventory.InventoryItems.FirstOrDefault(pair => pair.InventoryObject.Id == inventoryObject.Id);
				Assert.NotNull(item);
				Assert.AreEqual(count, item.ItemContext.CurrentQuantity.Value, $"expected {count} {inventoryObject.Id} | actual {item.ItemContext.CurrentQuantity.Value} {item.Id}");
			}
		});
	}
}