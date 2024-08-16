using Core.Entity;
using Core.Factory;
using Core.Loot;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Common.SpawnPoint
{
	public class UniqueLootObjectSpawnPoint : ObjectSpawnPoint
	{
		[ValueDropdown("@UniqueMapObject.PrefKeys")]
		[SerializeField] private string _uniqueKey;
        
		public override EntityContext CreateObject(IObjectFactory objectFactory, bool destroy)
		{
			var loot = base.CreateObject(objectFactory, destroy);
			OnCreated(loot);
			return loot;
		}
        

		private void OnCreated(object entity)
		{
			var loot = (UniqueLootEntity)entity;
			loot.SetKey(_uniqueKey);
		}
	}
}
