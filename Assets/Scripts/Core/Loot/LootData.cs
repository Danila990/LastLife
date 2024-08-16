using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Loot
{
	[CreateAssetMenu(menuName = SoNames.LOOT + nameof(LootData), fileName = nameof(LootData))]

	public class LootData : ScriptableObject
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Loot)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string[] GuaranteedLoot;
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Loot)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string[] RandomLoot;
		public int RandomQuantity = 3;
	}
}
