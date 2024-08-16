using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Loot;
using Db.ObjectData.Impl;
using UnityEngine;

namespace Core.Boosts.Entity
{
	public class BoostLoot : LootEntity
	{
		[SerializeField] private BoostItemObjectDataSo _boostSo;
		
		protected override void OnInteractWithPlayer(CharacterContext context)
		{
			if (context.Adapter is PlayerCharacterAdapter player)
			{
				player.BoostsInventory.Add(_boostSo.Model.BoostArgs);
			}
		}
	}
}
