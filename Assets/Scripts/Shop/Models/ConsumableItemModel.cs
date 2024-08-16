using System;
using Db.ObjectData;
using Sirenix.OdinInspector;

namespace Shop.Models
{
	[Serializable]
	public class ConsumableItemModel<T> : ShopObjectItemModel<T> where T : ObjectData
	{
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public int Count;
	}
}