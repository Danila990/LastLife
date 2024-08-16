using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction;
using Core.Entity.EntityUpgrade.Upgrades.Abs;
using Core.Inventory;
using Core.Inventory.Items.Weapon;
using SharedUtils.PlayerPrefs;
using SharedUtils.PlayerPrefs.Impl;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.EntityUpgrade
{
	[CreateAssetMenu(menuName = SoNames.UPGRADES + "CharacterUpgrades", fileName = "CharacterUpgrades")]
	public class CharacterUpgrades : AbstractUpgradesList
	{
		[SerializeField] private string _uniqueId;
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField] private string _targetId;
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField] private string _playerId;
		[SerializeField] private SpecialUpgradeData[] _specialUpgrades;
		[OdinSerialize, ListDrawerSettings(ListElementLabelName = "PrefKey")] private EntityUpgradeParameters[] _upgrades;
		public override string UniquePrefId => _uniqueId;
		public string TargetId => _targetId;
		public string PlayerId => _playerId;

		public override void Initialize(IInventory inventory, IPlayerPrefsManager playerPrefsManager)
		{
			ApplyUpgrades(inventory, playerPrefsManager);
			var entity = (IUpgradableEntity)inventory.InventoryItems.FirstOrDefault(pair => pair.InventoryObject.Id == _targetId).ItemContext;
			entity.OnUpgradesInit();
		}
		
		public override IReadOnlyList<SpecialUpgrade> GetSpecialUpgrades(IObjectResolver objectResolver)
		{
			var upgrades = new List<SpecialUpgrade>(_specialUpgrades.Length);
			
			foreach (var specialUpgrade in _specialUpgrades)
			{
				upgrades.Add(specialUpgrade.GetSpecialUpgrade(objectResolver));
			}
			
			return upgrades;
		}
		
		public override void ApplyUpgrades(IInventory inventory, IPlayerPrefsManager playerPrefsManager)
		{
			var upgradableEntity = (IUpgradableEntity)inventory.InventoryItems.FirstOrDefault(pair => pair.InventoryObject.Id == _targetId).ItemContext;
			if (upgradableEntity == null)
				return;
			
			upgradableEntity.ResetUpgrades();
			ref var upgradableEntityParameters = ref upgradableEntity.Parameters;
			
			foreach (var upgrade in _upgrades)
			{
				var upgradeLevel = GetEntityUpgradeLevel(upgrade, playerPrefsManager);
				upgrade.ApplyUpgrade(ref upgradableEntityParameters, upgradeLevel);
			}
			
			upgradableEntity.OnUpgrade();
		}

		public IEnumerable<EntityUpgradeParameters> GetEntityUpgrades() => _upgrades;
		
		public int GetEntityUpgradeLevel(IEntityUpgrade entityUpgrade, IPlayerPrefsManager playerPrefsManager) 
			=> playerPrefsManager.GetValue<int>(UpgradePrefKey(entityUpgrade), 0);

		public void SetEntityUpgradeLevel(IEntityUpgrade entityUpgrade, int level, IPlayerPrefsManager playerPrefsManager)
			=> playerPrefsManager.SetValue(UpgradePrefKey(entityUpgrade), level);
		
		private string UpgradePrefKey(IEntityUpgrade upgrade) 
			=> UniquePrefId + "_" + upgrade.PrefKey;
		
		public bool EntityUpgradeIsMaxLevel(EntityUpgradeParameters entityUpgrade, IPlayerPrefsManager playerPrefsManager) 
			=> GetEntityUpgradeLevel(entityUpgrade, playerPrefsManager) >= entityUpgrade.MaxLevel;

#if UNITY_EDITOR
		[ShowInInspector, DisplayAsString]
		public string CurrentFreePoints => GetUpgradePoints(new PersistentPlayerPrefsManager()).ToString();
		
		[ShowInInspector, NonSerialized, InlineEditor] public ProjectileWeaponContext Item;
		
		[Button]
		private void SetUpPoints(int level)
		{
			SetUpgradePoints(new PersistentPlayerPrefsManager(), level);
		}
		
		
		[Button]
		private void SetUpgradesLevels(int level)
		{
			foreach (var upgrade in _upgrades)
			{
				PlayerPrefs.SetInt(_uniqueId + "_" + upgrade.PrefKey, level);
			}
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			if (_upgrades == null || _upgrades.Length == 0)
				return;
			
			foreach (var upgrade in _upgrades)
			{
				if (upgrade is null)
					continue;
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField(new GUIContent(upgrade.PrefKey), GUILayout.MaxWidth(100));
				
				GUI.color = Color.green;
				EditorGUILayout.LabelField(
					new GUIContent($"lvl:{ UnityEngine.PlayerPrefs.GetInt(_uniqueId + "_" + upgrade.PrefKey, 0)}"), 
					GUILayout.MaxWidth(100));
				GUI.color = Color.white;

				EditorGUILayout.EndHorizontal();
			}
		}
#endif
	}
}