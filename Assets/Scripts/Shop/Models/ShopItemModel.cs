using System;
using System.Linq;
using Shop.Abstract;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shop.Models
{
	[Serializable]
	public class ShopItemModel : IPurchasable
	{
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		[field:SerializeField, ValueDropdown("@AInAppDatabase.AllIds"), GUIColor("GetColor")]
		public string InAppId { get; private set; }
		
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public string Price;
		
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public float PriceUSDNumber;
		
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Right")]
		[BoxGroup("Info/Split/Right/Preview")]
		[AssetSelector(Paths = "Assets/Sprites/Shop", FlattenTreeView = true)]
		[PreviewField] 
		public Sprite Ico;
		
#if UNITY_EDITOR
		[BoxGroup("Editor")]
		[HorizontalGroup("Editor/H")]
		[Button]
		public void RefreshInApp()
		{
			AInAppDatabase.EditorInstance.UpdateValues();
		}
		
		public Color GetColor()
		{
			return AInAppDatabase.EditorInstance.GetProducts().Any(x => x.InAppID == InAppId) ? Color.green : Color.magenta;
		}
		
		[BoxGroup("Editor")]
		[HorizontalGroup("Editor/H")]
		[Button(ButtonStyle.CompactBox, DirtyOnClick = true, Expanded = true)]
		public void SetTestId(string id)
		{
			InAppId = id;
		}
#endif
	}
}