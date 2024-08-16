using UnityEngine;
using Utils;

namespace Db.ObjectData.Impl
{
	[CreateAssetMenu(menuName = SoNames.OBJECT_MODEL_DATA + nameof(InventoryObjectDataSo), fileName = "InventoryObjectDataSo")]
	public class InventoryObjectDataSo : ObjectDataSo<InventoryObjectData>
	{
		
	}
}