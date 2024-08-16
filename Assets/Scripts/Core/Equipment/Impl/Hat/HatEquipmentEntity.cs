using Core.Equipment.Data;
using UnityEngine;

namespace Core.Equipment.Impl.Hat
{
	public abstract class HatEquipmentEntity : EquipmentEntityContext
	{
		protected HatItemArgs CurrentItemArgs;

		public override IEquipmentArgs GetItemArgs()
			=> CurrentItemArgs;
		
		public override void ChangeCurrentArgs(in IEquipmentArgs args)
		{
			if (args is HatItemArgs bootsArgs)
			{
				CurrentItemArgs = bootsArgs;
				return;
			}

			Debug.LogError($"[{nameof(HatEquipmentEntity)}] Incorrect arguments({args}) for an item of equipment");
		}

		protected override void PlaceCosmetic()
		{
			if (Inventory.TryGetOrigin(PartType, out var data))
			{
				var placementData = data.GetOffset(CurrentItemArgs.FactoryId);

				MainTransform.SetParent(placementData.Origin);
				MainTransform.localPosition = placementData.Offset;
				MainTransform.localEulerAngles = placementData.Rotation;
			}
		}
	}
}
