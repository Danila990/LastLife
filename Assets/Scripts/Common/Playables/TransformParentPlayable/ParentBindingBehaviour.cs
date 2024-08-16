using System;
using TweenPlayables;

namespace Common.Playables.TransformParentPlayable
{
	[Serializable]
	public class ParentBindingBehaviour : TweenAnimationBehaviour<ParentBinding>
	{
		public FloatTweenParameter FloatTweenParameter;
		
		/*public override void OnTweenInitialize(ParentBinding playerData)
		{
			var lerp = (playerData.Source.position - playerData.Source.parent.position).magnitude / (playerData.Target.position - playerData.Source.parent.position).magnitude;
			FloatTweenParameter.SetInitialValue(playerData, lerp);
		}*/
	}
}