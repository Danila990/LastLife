using System.ComponentModel;
using TweenPlayables;
using UnityEngine.Timeline;

namespace Common.Playables.TweenRenderer
{
	[TrackBindingType(typeof(TweenRendererBinding))]
	[TrackClipType(typeof(TweenMaterialRendererClip))]
#if UNITY_EDITOR
	[DisplayName("Tween Playables/General/Material Renderer")]
#endif
	public class TweenMaterialTrack : TweenAnimationTrack<TweenRendererBinding, TweenMaterialMixerBehaviour, RendererMaterialBehaviour>
	{
		
	}
}