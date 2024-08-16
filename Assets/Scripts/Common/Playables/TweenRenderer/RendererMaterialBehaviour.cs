using System;
using TweenPlayables;

namespace Common.Playables.TweenRenderer
{
	[Serializable]
	public class RendererMaterialBehaviour : TweenAnimationBehaviour<TweenRendererBinding>
	{
		public FloatTweenParameter FloatTweenParameter;
		
		public override void OnTweenInitialize(TweenRendererBinding playerData)
		{
			FloatTweenParameter.SetInitialValue(playerData, playerData.Disposable.materialsCopy[0].GetFloat(playerData.Property));
		}
	}
}