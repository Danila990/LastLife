namespace Tests.Sandbox
{
	/*public class EquipmentInventoryTest : UiTest
	{
		[UnityTest]
		public IEnumerator SelectDeselectAll() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			var addedCount = 0;
			var storeCount = 0;
			foreach (var kpv in itemStorage.EquipmentByType)
			{
				foreach (var data in kpv.Value)
				{
					context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
					addedCount++;
				}
			}
			await UniTask.Delay(500, cancellationToken: CancellationToken);
			foreach (var items in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType.Values)
			{
				storeCount += items.Count;
			}
			Assert.AreEqual(addedCount, storeCount);
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			foreach (var kpv in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType)
			{
				foreach (var args in kpv.Value)
				{
					context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
					await UniTask.Delay(1000, cancellationToken: CancellationToken);

					Assert.IsTrue(context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType));

					await UniTask.Delay(1000, cancellationToken: CancellationToken);
				}
			}

		});
		
		[UnityTest]
		public IEnumerator BulletproofRestartDueToDeath() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var characterMenuController = scope.Container.Resolve<CharacterMenuController>();

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var kpv in itemStorage.EquipmentByType)
			{
				foreach (var data in kpv.Value)
				{
					if(data.Args is BulletproofItemArgs)
						context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
				}
			}
			
			foreach (var kpv in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType)
			{
				foreach (var args in kpv.Value)
				{
					context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
					await UniTask.Delay(1000, cancellationToken: CancellationToken);

					if (args.PartType == EquipmentPartType.Body
					    && context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.Body] is BulletproofVestEntity)
					{
						context.EquipmentInventory.TryGetParts(EquipmentPartType.Body, out var parts);
						foreach (var part in parts)
						{
							var dmg = new DamageArgs()
							{
								Damage = 5f,
							};
							
							part.DoDamage(ref dmg, DamageType.Generic);
						}
					}
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
					context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
				}
			}
			var dmge = new DamageArgs()
			{
				Damage = context.Health.CurrentHealth,
			};
			context.Health.DoDamage(ref dmge);
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			characterMenuController.ClickPlayForTest();
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			foreach (var items in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType.Values)
			{
				foreach (var args in items)
				{
					if (args is BulletproofItemArgs bulletproofItemArgs)
					{
						var storedArgs = 
							(BulletproofItemArgs)itemStorage.EquipmentByType[args.PartType]
								.First(x => x.Args.GetItemId() == bulletproofItemArgs.GetItemId()).Args;
						
						Debug.Log($"{bulletproofItemArgs.Health} == {storedArgs.Health}");
						Assert.AreEqual(bulletproofItemArgs.Health, storedArgs.Health);
						await UniTask.Delay(100, cancellationToken: CancellationToken);
					}
				}
			}
		});
		
		[UnityTest]
		public IEnumerator BulletproofRestartDueToSelect() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var characterMenuController = scope.Container.Resolve<CharacterMenuController>();
			var msger = scope.Container.Resolve<IUiMessagesPublisherService>();
			
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			
			foreach (var kpv in itemStorage.EquipmentByType)
			{
				foreach (var data in kpv.Value)
				{
					if(data.Args is BulletproofItemArgs)
						context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
				}
			}
			
			foreach (var kpv in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType)
			{
				foreach (var args in kpv.Value)
				{
					context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
					await UniTask.Delay(1000, cancellationToken: CancellationToken);

					if (args.PartType == EquipmentPartType.Body
					    && context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.Body] is BulletproofVestEntity)
					{
						context.EquipmentInventory.TryGetParts(EquipmentPartType.Body, out var parts);
						foreach (var part in parts)
						{
							var dmg = new DamageArgs()
							{
								Damage = 5f,
							};
							
							part.DoDamage(ref dmg, DamageType.Generic);
						}
					}
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
					context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
				}
			}
			
			msger.OpenWindowPublisher.OpenWindow<CharacterMenuWindow>();
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			characterMenuController.ClickPlayForTest();
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			foreach (var items in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType.Values)
			{
				foreach (var args in items)
				{
					if (args is BulletproofItemArgs bulletproofItemArgs)
					{
						var storedArgs = (BulletproofItemArgs)itemStorage.EquipmentByType[args.PartType].First(x => x.Args.GetItemId() == bulletproofItemArgs.GetItemId()).Args;
						Debug.Log($"{bulletproofItemArgs.Health} != {storedArgs.Health}");
						Assert.AreNotEqual(bulletproofItemArgs.Health, storedArgs.Health);
						await UniTask.Delay(100, cancellationToken: CancellationToken);
					}
				}
			}
		});
		
		[UnityTest]
		public IEnumerator JetPackRestartDueToDeath() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var characterMenuController = scope.Container.Resolve<CharacterMenuController>();

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var data in itemStorage.EquipmentByType[EquipmentPartType.JetPack])
			{
				if(data.Args is JetPackItemArgs)
					context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
			}
			
			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
				context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
				await UniTask.Delay(1000, cancellationToken: CancellationToken);

				var jetpack = (JetPackContext)context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.JetPack];
				jetpack.ConsumeFuelTest();
				NUnit.Framework.Assert.LessOrEqual(0f, jetpack.Args.CurrentFuel.Value);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
				context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
			}

			var dmg = new DamageArgs()
			{
				Damage = context.Health.CurrentHealth
			};
			context.Health.DoDamage(ref dmg);
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			
			//TODO: FIX
			//CheatSettings.UnlockAllTest(true);
			characterMenuController.ClickPlayForTest();
			
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			await UniTask.Delay(1000, cancellationToken: CancellationToken);

			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
				var jetPackItemArgs = args as JetPackItemArgs;
				NUnit.Framework.Assert.IsNotNull(jetPackItemArgs);
				var refItem = (JetPackItemArgs)itemStorage.EquipmentByType[EquipmentPartType.JetPack].First(x => x.Id == jetPackItemArgs.GetItemId()).GetArgs();
				NUnit.Framework.Assert.AreEqual(
					refItem.Fuel,
					jetPackItemArgs.Fuel
				);
				Debug.Log($"{refItem.Fuel} == {jetPackItemArgs.Fuel}");
			}
			
			//CheatSettings.UnlockAllTest(false);
		});
		
		[UnityTest]
		public IEnumerator JetPackRestartDueToSelect() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var characterMenuController = scope.Container.Resolve<CharacterMenuController>();

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var data in itemStorage.EquipmentByType[EquipmentPartType.JetPack])
			{
				if(data.Args is JetPackItemArgs)
					context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
			}
			
			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
					context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
					await UniTask.Delay(1000, cancellationToken: CancellationToken);

					var jetpack = (JetPackContext)context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.JetPack];
					jetpack.ConsumeFuelTest();
					NUnit.Framework.Assert.LessOrEqual(0f, jetpack.Args.CurrentFuel.Value);
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
					context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
			}
			
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			characterMenuController.ClickPlayForTest();
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
				var jetPackItemArgs = args as JetPackItemArgs;
				NUnit.Framework.Assert.IsNotNull(jetPackItemArgs);
				var refItem = (JetPackItemArgs)itemStorage.EquipmentByType[EquipmentPartType.JetPack].First(x => x.Id == jetPackItemArgs.GetItemId()).GetArgs();
				Debug.Log($"{refItem.Fuel} > {jetPackItemArgs.Fuel}");
				NUnit.Framework.Assert.Greater(
					refItem.Fuel,
					jetPackItemArgs.Fuel
				);
			}
		});
		
		[UnityTest]
		public IEnumerator JetPackRefuel() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var fuelService = scope.Container.Resolve<IFuelService>();

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var data in itemStorage.EquipmentByType[EquipmentPartType.JetPack])
			{
				if(data.Args is JetPackItemArgs)
					context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
			}

			var tankCounts = 0; 
			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
					context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
					await UniTask.Delay(1000, cancellationToken: CancellationToken);

					var jetpack = (JetPackContext)context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.JetPack];
					jetpack.ConsumeAllFuelTest();
					fuelService.AddTank(new FuelTank(){ Fuel = 20f });
					tankCounts++;
					NUnit.Framework.Assert.LessOrEqual(0f, jetpack.Args.CurrentFuel.Value);
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
					context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
					await UniTask.Delay(1000, cancellationToken: CancellationToken);
			}
			
			NUnit.Framework.Assert.AreEqual(tankCounts, fuelService.TanksCount.Value);
			
			
			await UniTask.Delay(1000, cancellationToken: CancellationToken);

			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
				context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
				await UniTask.Delay(1000, cancellationToken: CancellationToken);

				var jetpack = (JetPackContext)context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.JetPack];
				jetpack.AddFuel(fuelService.GetTank().Fuel);
				NUnit.Framework.Assert.Greater(jetpack.Args.CurrentFuel.Value, 0f);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
				context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
			}
			NUnit.Framework.Assert.AreEqual(0, fuelService.TanksCount.Value);
		});
		
		[UnityTest]
		public IEnumerator JetPackFullRefuel() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var fuelService = scope.Container.Resolve<IFuelService>();

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var data in itemStorage.EquipmentByType[EquipmentPartType.JetPack])
			{
				if(data.Args is JetPackItemArgs)
					context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
			}

			var tankCounts = 0; 
			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
				fuelService.AddTank(new FuelTank(){Fuel = 20f});
				tankCounts++;
			}
			
			NUnit.Framework.Assert.AreEqual(tankCounts, fuelService.TanksCount.Value);
			
			
			await UniTask.Delay(1000, cancellationToken: CancellationToken);

			foreach (var args in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType[EquipmentPartType.JetPack])
			{
				context.EquipmentInventory.Controller.ActiveEquipment.Select(args.PartType, args.GetItemId());
				await UniTask.Delay(1000, cancellationToken: CancellationToken);

				var jetpack = (JetPackContext)context.EquipmentInventory.Controller.ActiveEquipment.Equipment[EquipmentPartType.JetPack];
				jetpack.AddFuel(fuelService.GetTank().Fuel);
				NUnit.Framework.Assert.AreEqual(jetpack.Args.CurrentFuel.Value, jetpack.Args.MaxFuel);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
				context.EquipmentInventory.Controller.ActiveEquipment.Deselect(args.PartType);
				await UniTask.Delay(1000, cancellationToken: CancellationToken);
			}
			NUnit.Framework.Assert.AreEqual(0, fuelService.TanksCount.Value);
		});
		
		[UnityTest]
		public IEnumerator ClearLockedItemsDueToDeath() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var itemStorage = scope.Container.Resolve<IItemStorage>();
			var unlockService = scope.Container.Resolve<IItemUnlockService>();
			var characterMenuController = scope.Container.Resolve<CharacterMenuController>();

			await UniTask.Delay(500, cancellationToken: CancellationToken);

			playerSpawnService.CreatePlayerContext("AWPMan");
			await UniTask.Delay(500, cancellationToken: CancellationToken);

			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			foreach (var kpv in itemStorage.EquipmentByType)
			{
				foreach (var data in kpv.Value)
				{
					context.EquipmentInventory.Controller.AllEquipment.AddPart(data.GetArgs());
				}
			}
			
			var dmge = new DamageArgs()
			{
				Damage = context.Health.CurrentHealth,
			};
			context.Health.DoDamage(ref dmge);
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			characterMenuController.ClickPlayForTest();
			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;

			foreach (var items in context.EquipmentInventory.Controller.AllEquipment.EquipmentByType.Values)
			{
				foreach (var args in items)
				{
					if (args is BulletproofItemArgs bulletproofItemArgs)
					{
						var storedItem = 
							itemStorage.EquipmentByType[args.PartType]
								.First(x => x.Args.GetItemId() == bulletproofItemArgs.GetItemId());

						Debug.Log(storedItem.Id);
						Assert.IsTrue(unlockService.IsUnlocked(storedItem));
						await UniTask.Delay(100, cancellationToken: CancellationToken);
					}
				}
			}
		});
	}*/
}