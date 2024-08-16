using Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction;
using Core.Inventory;
using Core.Services;
using Db.ObjectData;

namespace Core.Entity.EntityUpgrade.SpecialUpgrades.Impl
{
	public class AddActionUpgrade : SpecialUpgrade
	{
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _itemUnlockService;
		private bool _isUnlocked;
		private AbilityObjectData _targetedAbilityObject;
		public AddActionUpgradeData Data { get; }
		
		public AddActionUpgrade(AddActionUpgradeData data, IItemStorage itemStorage, IItemUnlockService itemUnlockService)
		{
			_itemStorage = itemStorage;
			_itemUnlockService = itemUnlockService;
			Data = data;
		}		
		
		public override void Initialize(BaseInventory inventory)
		{
			_targetedAbilityObject = _itemStorage.AbilityObjects[Data.TargetAbilityToUnlock];
			_isUnlocked = _itemUnlockService.IsUnlocked(_targetedAbilityObject);
		}
		
		public override void OnLevelChanged(int level)
		{
			if (!_isUnlocked)
			{
				if (level >= Data.MinLevelToUnlock)
				{
					_isUnlocked = true;
					_itemUnlockService.UnlockItem(_targetedAbilityObject);
				}
			}
		}
		public override void Dispose()
		{
		}
	}
}