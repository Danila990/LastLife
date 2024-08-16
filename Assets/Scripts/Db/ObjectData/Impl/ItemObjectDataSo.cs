using UnityEngine;
using Utils;

namespace Db.ObjectData.Impl
{
	[CreateAssetMenu(menuName = SoNames.OBJECT_MODEL_DATA + nameof(ItemObjectDataSo), fileName = "ItemObjectDataSo")]
	public class ItemObjectDataSo : ObjectDataSo<ItemObject>
	{
	
	}
}