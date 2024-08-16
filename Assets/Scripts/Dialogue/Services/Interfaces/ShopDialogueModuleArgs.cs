using Common.SpawnPoint;
using NodeCanvas.Framework;
using UnityEngine;

namespace Dialogue.Services.Interfaces
{
	public class ShopDialogueModuleArgs : MonoModuleDialogueArgs
	{
		public MerchantShopItemPointManager ShopItemPointManager { get; set; }
		public MerchantPlaceInfo MerchantPlaceInfo { get; set; }
		public Blackboard Blackboard;
		
		public Vector3 GetSpawnedItemPosition()
		{
			return ShopItemPointManager.GetSpawnedItemPosition();
		}
	}
}