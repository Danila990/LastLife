using System.ComponentModel;
using TweenPlayables;
using UnityEngine.Timeline;

namespace Common.Playables.TransformParentPlayable
{
	[TrackBindingType(typeof(ParentBinding))]
	[TrackClipType(typeof(ParentBindingClip))]
#if UNITY_EDITOR
	[DisplayName("Tween Playables/ParentBinding")]
#endif
	public class ParentBindingTrack : TweenAnimationTrack<ParentBinding, ParentBindingMixer, ParentBindingBehaviour>
	{
		
	}
}