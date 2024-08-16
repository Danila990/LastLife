using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db.Panel
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(ItemsPanel), fileName =  nameof(ItemsPanel))]
	public class ItemsPanel : ScriptableObject
	{
		public string PanelId;
		public Sprite PanelIcon;
		public string PanelName;
		public ElementItemPanel[] ElementItems;
		public ItemCategory[] CategoriesData;
	}
	
	[Serializable]
	public class ItemCategory
	{
		public string CategoryId;
		public Sprite Ico;
	}

	[Serializable]
	public class ElementItemPanel
	{
		public string CategoryId; 
		[ValueDropdown("@ObjectsData.AllIds")]
		[InlineButton("@Db.ObjectData.ObjectsData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string ItemObjectDataId;
		
		public ElementType ElementType;
	}

	public enum ElementType
	{
		InventoryAdd,
		ForSpawning
	}
}