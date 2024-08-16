using System.Collections.Generic;
using Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction;
using Core.Inventory;
using SharedUtils.PlayerPrefs;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity.EntityUpgrade
{
	public class UpgradeController : MonoBehaviour
	{
		[SerializeField] private BaseInventory _inventory;
		[SerializeField] private AbstractUpgradesList _upgradesList;
		private IReadOnlyList<SpecialUpgrade> _specialUpgrades;
		private IPlayerPrefsManager _prefs;

		public AbstractUpgradesList UpgradesList => _upgradesList;
		
		public void Initialize(IControllableEntity owner, IPlayerPrefsManager prefs, IObjectResolver resolver)
		{
			_prefs = prefs;
			if (!_upgradesList)
				return;
			
			_specialUpgrades = _upgradesList.GetSpecialUpgrades(resolver);
			
			foreach (var specialUpgrade in _specialUpgrades)
			{
				specialUpgrade.AddTo(this);
				resolver.Inject(specialUpgrade);
				specialUpgrade.Initialize(_inventory);
			}
			
			_upgradesList.Initialize(_inventory, _prefs);
		}
		
		[Button]
		public void OnLevelChanged(int level)
		{
			foreach (var specialUpgrade in _specialUpgrades)
			{
				specialUpgrade.OnLevelChanged(level);
			}
		}
	}
}