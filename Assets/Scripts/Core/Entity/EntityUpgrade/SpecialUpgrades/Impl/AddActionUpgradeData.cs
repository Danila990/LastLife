using Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction;
using Core.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.EntityUpgrade.SpecialUpgrades.Impl
{
	[CreateAssetMenu(menuName = SoNames.SPECIAL_UPGRADES + nameof(AddActionUpgradeData), fileName = "AddActionUpgradeData")]
	public class AddActionUpgradeData : SpecialUpgradeData
	{
		[field:ValueDropdown("@ObjectsData.GetAbilities()")]
		[field:SerializeField]
		public string TargetAbilityToUnlock { get; set; }
		
		public override SpecialUpgrade GetSpecialUpgrade(IObjectResolver objectResolver)
		{
			return new AddActionUpgrade(
				this, 
				objectResolver.Resolve<IItemStorage>(),
				objectResolver.Resolve<IItemUnlockService>());
		}
	}
}