using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.EntityUpgrade;
using Core.Entity.EntityUpgrade.Upgrades.Impl;
using Core.Factory;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Tests.UITest;
using UnityEngine;
using UnityEngine.TestTools;
using Utils.Constants;
using VContainer;

namespace Tests.Sandbox
{
	public class UpgradesTests : UiTest 
	{
		protected override void OverrideRegistration()
		{
			PlayerPrefs.DeleteAll();
		}

		[UnityTest]
		public IEnumerator TestUpgradesWork() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var strategyFactory = scope.Container.Resolve<IAdapterStrategyFactory>();
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var upgradesData = scope.Container.Resolve<ICharacterUpgradesData>();
			var allUpgrades = upgradesData.Upgrades;
			DestroyAllLifeScene(scope.Container, true);
			
			foreach (var upgrade in allUpgrades)
			{
				var dict = new Dictionary<string, float>()
				{
					{ WeaponConsts.CAPACITY, 0 },
					{ WeaponConsts.DAMAGE, 0 },
					{ WeaponConsts.DISTANCE, 0},
					{ WeaponConsts.ACCURACY, 0},
					{ WeaponConsts.SPIN, 0 },
					{ WeaponConsts.FIRE_RATE, 0 }
				};
				var parameters = new UpgradeableParameters(dict);
				var defaultValues = parameters.GetParameters.ToArray();
				var changedValues = new List<string>();
				
				foreach (var up in upgrade.GetEntityUpgrades())
				{
					if (up is WeaponParameterUpgradeParameters weaponParameterUpgradeParameters)
					{
						changedValues.Add(up.PrefKey);
						weaponParameterUpgradeParameters.ApplyUpgrade(ref parameters, up.MaxLevel);
					}
				}
				
				foreach (var parameter in defaultValues)
				{
					var value = parameters.GetValue(parameter.Key);
					if (changedValues.Contains(parameter.Key))
					{
						Assert.AreNotEqual(value, parameter.Value);
					}
					else
					{
						Assert.AreEqual(value, parameter.Value);
					}
				}
				
				changedValues.Clear();
			}
		});
	}
}