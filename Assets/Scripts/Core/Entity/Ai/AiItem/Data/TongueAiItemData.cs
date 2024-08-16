using Core.Entity.Head;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.AiItem.Data
{

	[CreateAssetMenu(menuName = SoNames.AI_ITEM_DATA + nameof(TongueAiItemData), fileName = nameof(TongueAiItemData))]
	public class TongueAiItemData : GenericAiItemData, ITongueAiItemData
	{
		[field:SerializeField] public float HandleForwardDuration { get; private set; }
		[field:SerializeField] public float ForwardSpeed { get; private set; }
		[field:SerializeField] public float HandleBackDuration { get; private set; }
		[field:SerializeField] public float BackSpeed { get; private set; }
		[field:SerializeField] public AudioClip StartEat { get; private set; }
		[field:SerializeField] public AudioClip EndEat { get; private set; }

		[field:SerializeField] public float DistanceThreshold  { get; private set; }
		
		[field:SerializeField, ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
		public string MeatFXName { get; private set; }

		public override AbstractAiItem CreateAiItem(EntityContext owner)
		{
			var tongue = new TongueAiItem(owner as HeadContext, this);
			tongue.Created();
			return tongue;
		}
	}
	
	public interface ITongueAiItemData : IAiItemData
	{
		float HandleForwardDuration { get; }
		float ForwardSpeed { get; }
		float HandleBackDuration { get; }
		float BackSpeed { get; }
		AudioClip StartEat { get; }
		AudioClip EndEat { get; }
		float DistanceThreshold { get; }
		string MeatFXName { get; }
	}
}