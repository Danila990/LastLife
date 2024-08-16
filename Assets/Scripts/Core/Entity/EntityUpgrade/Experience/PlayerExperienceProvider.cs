using System.Linq;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction;
using Core.Services;
using SharedUtils.PlayerPrefs;
using Shop;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Entity.EntityUpgrade.Experience
{
	public class PlayerExperienceProvider : ExperienceProvider
	{
		public override int CurrentLevel
		{
			get => GetLevel();
			set => SetLevel(value);
		}
		
		public override float CurrentExperience
		{
			get => GetExperience();
			set => SetExperience(value);
		}

		public override int MaxLevel => 15;
		public override bool IsAvailable => _isAvailable;
		public override string UniqueId => _upgradeList.UniquePrefId;
		public override void AddPoint(int i)
		{
			var points = _upgradeList.GetUpgradePoints(_playerPrefsManager);
			_upgradeList.SetUpgradePoints(_playerPrefsManager, points + i);
		}
		
		public override int AvailablePoints => _upgradeList.GetUpgradePoints(_playerPrefsManager);
		
		public override void OnEntityUpgraded(CharacterUpgrades upgrade)
		{
			if (_upgradeList.UniquePrefId == upgrade.UniquePrefId)
			{
				_upgradeList.ApplyUpgrades(_characterContext.Inventory, _playerPrefsManager);
			}
		}

		private IPlayerPrefsManager _playerPrefsManager;
		private UpgradeController _currentController;
		private IObjectResolver _resolver;
		private CharacterContext _characterContext;
		private bool _isAvailable;
		private AbstractUpgradesList _upgradeList;
		private bool _isBought;
		private IItemUnlockService _itemUnlockService;
		private PlayerCharacterAdapter _playerCharacterAdapter;
		private IItemStorage _itemStorage;

		public void Initialize(IPlayerPrefsManager playerPrefsManager, IObjectResolver resolver, PlayerCharacterAdapter playerCharacterAdapter)
		{
			_playerCharacterAdapter = playerCharacterAdapter;
			_resolver = resolver;
			_playerPrefsManager = playerPrefsManager;
			_itemUnlockService = _resolver.Resolve<IItemUnlockService>();
			_itemStorage = _resolver.Resolve<IItemStorage>();
		}
		
		[Button]
		public void SetLevel(int level)
		{
			if (!_isAvailable)
				return;
			
			_currentController.OnLevelChanged(level);
			_playerPrefsManager.SetValue<int>(GetLevelPrefKey(), level);
		}

		public int GetLevel()
		{
			if (!_isAvailable)
				return 0;
			
			return _playerPrefsManager.GetValue<int>(GetLevelPrefKey(), 0);
		}

		public void OnEntityChanged(CharacterContext characterContext)
		{
			var item = _itemStorage.Characters.Values.FirstOrDefault(data => data.PlayerId == _playerCharacterAdapter.ContextId);
			_characterContext = characterContext;
			_isAvailable = _characterContext.UpgradeController.UpgradesList && _itemUnlockService.IsUnlocked(item);
			_upgradeList = _characterContext.UpgradeController.UpgradesList;
			if (_isAvailable)
			{
				_currentController = _characterContext.UpgradeController;
				_currentController.Initialize(characterContext, _playerPrefsManager, _resolver);
				_currentController.OnLevelChanged(GetLevel());
			}
		}
		
		[Button]
		public void SetExperience(float experience)
		{
			if (!_isAvailable)
				return;
			
			_playerPrefsManager.SetValue<float>(GetExperiencePrefKey(), experience);
		}
		
		public float GetExperience()
		{
			if (!_isAvailable)
				return 0;
			
			return _playerPrefsManager.GetValue<float>(GetExperiencePrefKey(), 0);
		}
		
		private string GetLevelPrefKey() => _upgradeList.UniquePrefId + AdditionalPrefKeys.LEVEL;
		private string GetExperiencePrefKey() => _upgradeList.UniquePrefId + AdditionalPrefKeys.EXPERIENCE;
	}
}