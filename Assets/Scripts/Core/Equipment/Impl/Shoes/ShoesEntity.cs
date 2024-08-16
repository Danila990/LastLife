using Core.Equipment.Data;
using UnityEngine;

namespace Core.Equipment.Impl.Shoes
{
	public abstract class ShoesEntity : EquipmentEntityContext
	{
		protected BootsItemArgs CurrentItemArgs;

		public override IEquipmentArgs GetItemArgs()
			=> CurrentItemArgs;
		
		public override void ChangeCurrentArgs(in IEquipmentArgs args)
		{
			if (args is BootsItemArgs bootsArgs)
			{
				CurrentItemArgs = bootsArgs;
				return;
			}

			Debug.LogError($"[{nameof(ShoesEntity)}] Incorrect arguments({args}) for an item of equipment");
		}

		protected override void PlaceCosmetic()
		{
			if (Inventory.TryGetOrigin(PartType, out var data))
			{
				var placementData = data.GetOffset(CurrentItemArgs.FactoryId);

				MainTransform.SetParent(placementData.Origin);
				MainTransform.localPosition = placementData.Origin.localPosition + placementData.Offset;
				MainTransform.localEulerAngles = placementData.Rotation;
			}
			
			Equipmentizer.AttachTo(Inventory.Renderer);
		}
	}

}
