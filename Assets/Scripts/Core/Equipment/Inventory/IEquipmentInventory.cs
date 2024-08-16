using Core.Entity.InteractionLogic.Interactions;
using Core.Equipment.Data;
using UnityEngine;

namespace Core.Equipment.Inventory
{
	public interface IEquipmentInventory
	{
		public bool TryGetOrigin(EquipmentPartType type, out EquipmentOrigins origin);

		public bool TryGetParts(EquipmentPartType type, out CharacterPartArmored[] parts);

		public bool TryGetController(out EquipmentInventoryController controller);
		
		public SkinnedMeshRenderer Renderer { get; }
	}
}
