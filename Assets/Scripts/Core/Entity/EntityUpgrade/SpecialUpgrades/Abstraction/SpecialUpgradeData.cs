using UnityEngine;
using VContainer;

namespace Core.Entity.EntityUpgrade.SpecialUpgrades.Abstraction
{
	public abstract class SpecialUpgradeData : ScriptableObject
	{
		public int MinLevelToUnlock;

		public abstract SpecialUpgrade GetSpecialUpgrade(IObjectResolver objectResolver);
	}
}