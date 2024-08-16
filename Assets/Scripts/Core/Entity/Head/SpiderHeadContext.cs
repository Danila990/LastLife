using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Head
{
	public class SpiderHeadContext : HeadContext
	{
		[field:BoxGroup("Spider")]
		[field: SerializeField]
		public LegsAnimator LegsAnimator { get; private set; }
	}
}