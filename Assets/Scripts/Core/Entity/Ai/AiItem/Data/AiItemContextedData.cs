using Core.Inventory.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Ai.AiItem.Data
{
	public abstract class AiItemContextedData : ScriptableObject, IAiItemData
	{
		[field:TitleGroup("Main Properties")]
		[field:SerializeField] public float AttackRange { get; set; }
		[field:SerializeField] public bool SelfEnd { get; set; }
		[field:SerializeField] public float Cooldown { get; set; }
		[field:SerializeField] public float ConstPriority { get; set; }
		[field:SerializeField] public float UseItemDuration { get; set; }
		[field:SerializeField] public float Damage { get; set; }
		[field:SerializeField] public bool UseRig { get; set; }
		[field:SerializeField] public string RigName { get; set; }


		public abstract AiItemContextDecorator CreateAiItem(ItemContext itemContext, EntityContext owner);
		public abstract bool IsApplicable(ItemContext itemContext);
	}
}