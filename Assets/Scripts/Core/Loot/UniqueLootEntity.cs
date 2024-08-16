using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Loot
{
	public class UniqueLootEntity : LootEntity
	{
		private string _uniqueKey;

		public void SetKey(string key)
		{
			_uniqueKey = key;
			if (HasInPrefs() || string.IsNullOrEmpty(key))
			{
				OnDestroyed(EntityRepository);
				Destroy(gameObject);
			}
		}


		protected bool HasInPrefs() => PlayerPrefs.HasKey(_uniqueKey);
		protected void SetInPrefs() => PlayerPrefs.SetInt(_uniqueKey, 1);
	}
}
