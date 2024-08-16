using UnityEngine;
using Utils;

namespace Db.ObjectData.Impl
{
	[CreateAssetMenu(menuName = SoNames.OBJECT_MODEL_DATA + nameof(CharacterDataSo), fileName = "CharacterDataSo")]
	public class CharacterDataSo : ObjectDataSo<CharacterObjectData>
	{
		
	}
}