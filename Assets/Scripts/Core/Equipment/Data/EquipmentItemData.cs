using System;
using Db.ObjectData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Data
{
	public abstract class EquipmentItemData : ObjectData
	{
		public abstract IEquipmentArgs Args { get; }
	}

	public interface IEquipmentArgs
	{
		public EquipmentPartType PartType { get; }
		public string FactoryId { get; }
		public string Id { get; }

		public IEquipmentArgs GetCopy();
	}

	public abstract class EquipmentArgs : IEquipmentArgs
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Equipment)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string FactoryObjId;
		public string ItemId;
		[field: SerializeField] public EquipmentPartType PartType { get; set; }

		public string Id => ItemId;
		public string FactoryId => FactoryObjId;

		public abstract IEquipmentArgs GetCopy();

	}

	[Serializable]
	public class JetPackItemArgs : EquipmentArgs
	{
		[field: SerializeField] public float Fuel { get;  set; }

		public override IEquipmentArgs GetCopy()
		{
			return new JetPackItemArgs()
			{
				FactoryObjId = FactoryObjId,
				ItemId = ItemId,
				Fuel = Fuel,
				PartType = PartType
			};
		}
	}
	
	[Serializable]
	public class ArmorItemArgs : EquipmentArgs
	{
		[field: SerializeField] public float Health { get; set; }

		public override IEquipmentArgs GetCopy()
		{
			return new ArmorItemArgs()
			{
				FactoryObjId = FactoryObjId,
				ItemId = ItemId,
				Health = Health,
				PartType = PartType
			};
		}
	}
	
	[Serializable]
	public class BootsItemArgs : EquipmentArgs
	{

		[field: SerializeField] public float MovementSpeed { get; set; }
		[field: SerializeField] public float JumpHeight { get; set; }

		public override IEquipmentArgs GetCopy()
		{
			return new BootsItemArgs()
			{
				FactoryObjId = FactoryObjId,
				ItemId = ItemId,
				MovementSpeed = MovementSpeed,
				JumpHeight = JumpHeight,
				PartType = PartType
			};
		}
	}

	[Serializable]
	public class HatItemArgs : EquipmentArgs
	{
		public override IEquipmentArgs GetCopy()
		{
			return new HatItemArgs()
			{
				FactoryObjId = FactoryObjId,
				ItemId = ItemId,
				PartType = PartType
			};
		}
	}
	
	[Serializable]
	public enum EquipmentPartType
	{
		JetPack = 0,
		Body = 1,
		Foot = 2,
		Hat = 4,
		None = 5,
	}
}
