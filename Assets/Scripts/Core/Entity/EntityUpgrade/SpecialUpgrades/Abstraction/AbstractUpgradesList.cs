using System.Collections.Generic;
using Core.Inventory;
using SharedUtils.PlayerPrefs;
using Sirenix.OdinInspector;
using VContainer;

namespace Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction
{
	public abstract class AbstractUpgradesList : SerializedScriptableObject
	{
		public abstract void ApplyUpgrades(IInventory inventory, IPlayerPrefsManager playerPrefsManager);
		public abstract void Initialize(IInventory inventory, IPlayerPrefsManager playerPrefsManager);
		public abstract IReadOnlyList<SpecialUpgrade> GetSpecialUpgrades(IObjectResolver objectResolver);
		
		public abstract string UniquePrefId { get; }
		
		public int GetLevel(IPlayerPrefsManager playerPrefsManager) 
			=> playerPrefsManager.GetValue<int>(UniquePrefId + AdditionalPrefKeys.LEVEL, 0);
		
		public int GetUpgradePoints(IPlayerPrefsManager playerPrefsManager)
		{
			if (playerPrefsManager.HasKey(UniquePrefId + AdditionalPrefKeys.UPGRADE_POINTS))
			{
				return playerPrefsManager.GetValue<int>(UniquePrefId + AdditionalPrefKeys.UPGRADE_POINTS, 0);
			}
			else
			{
				var levelPoints = GetLevel(playerPrefsManager);
				playerPrefsManager.SetValue(UniquePrefId + AdditionalPrefKeys.UPGRADE_POINTS, levelPoints);
				return levelPoints;
			}
		}
		
		public void SetUpgradePoints(IPlayerPrefsManager playerPrefsManager, int points)
		{
			playerPrefsManager.SetValue(UniquePrefId + AdditionalPrefKeys.UPGRADE_POINTS, points);
		}
	}
}