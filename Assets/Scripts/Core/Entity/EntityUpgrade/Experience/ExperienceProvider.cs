using UnityEngine;

namespace Core.Entity.EntityUpgrade.Experience
{
	public abstract class ExperienceProvider : MonoBehaviour
	{
		public abstract int CurrentLevel { get; set; }
		public abstract float CurrentExperience { get; set; }
		public abstract int AvailablePoints { get;}
		public abstract int MaxLevel { get; }
		public abstract bool IsAvailable { get; }
		public abstract string UniqueId { get; }
		public abstract void AddPoint(int i);
		public abstract void OnEntityUpgraded(CharacterUpgrades upgrade);
	}
}