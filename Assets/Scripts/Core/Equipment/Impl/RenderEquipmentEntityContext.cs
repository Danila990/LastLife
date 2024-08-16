using System;
using Core.Equipment.Data;

namespace Core.Equipment.Impl
{

	public class RenderEquipmentEntityContext : EquipmentEntityContext
	{
		private IEquipmentArgs _args;
		
		public override void ChangeCurrentArgs(in IEquipmentArgs args) => _args = args;
		public override IEquipmentArgs GetItemArgs() => _args;

		protected override void PlaceCosmetic()
		{
			switch(PartType)
			{
				case EquipmentPartType.JetPack:
					EquipDefault();
					break;
				case EquipmentPartType.Body:
					EquipDefault();
					Equipmentizer.AttachTo(Inventory.Renderer);
					break;
				case EquipmentPartType.Foot:
					EquipDefault();
					Equipmentizer.AttachTo(Inventory.Renderer);
					break;
				case EquipmentPartType.Hat:
					EquipHat();
					break;
				case EquipmentPartType.None:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private void EquipHat()
		{
			if (Inventory.TryGetOrigin(PartType, out var data))
			{
				var placementData = data.GetOffset(_args.FactoryId);

				MainTransform.SetParent(placementData.Origin);
				MainTransform.localPosition = placementData.Offset;
				MainTransform.localEulerAngles = placementData.Rotation;
			}
		}
		
		private void EquipDefault()
		{
			if (Inventory.TryGetOrigin(PartType, out var data))
			{
				var placementData = data.GetOffset(_args.FactoryId);

				MainTransform.SetParent(placementData.Origin);
				MainTransform.localPosition = placementData.Origin.localPosition + placementData.Offset;
				MainTransform.localEulerAngles = placementData.Rotation;
			}
		}
	}
}