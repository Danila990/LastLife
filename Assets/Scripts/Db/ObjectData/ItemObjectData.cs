using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Db.ObjectData
{

	[Serializable]
	public class CharacterObjectData : ObjectData, IAiSpawnData
	{
		[VerticalGroup("Id")] 
		public string AdditionalName;

		[TitleGroup("Character")]
		public string PlayerId;

		public bool IsNew;
		
		[field: TitleGroup("Character")]
		[field: SerializeField]
		public string AiAdapterId { get; set; } //TODO: Add Dropdown
		
		[field: TitleGroup("Character")]
		[field: SerializeField] 
		public AiType AiType { get; set; }
	}

	public interface IAiSpawnData
	{
		string AiAdapterId { get; }
		AiType AiType { get; }
	}
	
	public enum AiType
	{
		Character,
		Head
	}
	
	[Serializable]
	public class ItemObject : ObjectData
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[VerticalGroup("Id")] public string FactoryId;
	}
	
	[Serializable]
	public class InventoryObjectData : ObjectData
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.InventoryItem)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[VerticalGroup("Id")] public string InventoryObjectId;
		public bool Savable;
	}
}