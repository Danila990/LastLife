using System;
using Db.ObjectData;
using Db.ObjectData.Impl;
using Sirenix.OdinInspector;

namespace Shop.Models
{
	[Serializable]
	public class ShopObjectItemModel<T> : ShopItemModel, IShopObjectItemModel
		where T : ObjectData
	{
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		[AssetSelector(Paths = "Assets/Settings/Data/ObjectsData/Characters")]
		public ObjectDataSo<T> Item;
		
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public bool IsNewItem;
		
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public bool IsBestSeller;
		public ObjectData ObjectData => Item.Model;
		public ShopItemModel ShopItemModel => this;
	}

	public interface IShopObjectItemModel
	{
		public ObjectData ObjectData { get; }
		public ShopItemModel ShopItemModel { get; }
	}
}