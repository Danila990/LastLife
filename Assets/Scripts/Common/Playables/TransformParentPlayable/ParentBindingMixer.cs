using TweenPlayables;
using UnityEngine;

namespace Common.Playables.TransformParentPlayable
{
	public class ParentBindingMixer : TweenAnimationMixerBehaviour<ParentBinding, ParentBindingBehaviour>
	{
		readonly FloatValueMixer floatValueMixer = new();

		public override void Blend(ParentBinding binding, ParentBindingBehaviour behaviour, float weight, float progress)
		{
			floatValueMixer.TryBlend(behaviour.FloatTweenParameter, binding, progress, weight);
		}
		
		public override void Apply(ParentBinding binding)
		{
			floatValueMixer.TryApplyAndClear(binding, (x, b) =>
			{
				b.Source.position = Vector3.LerpUnclamped(b.Source.parent.position, b.Target.position + b.Target.rotation * b.Offset, x);
				b.Source.rotation = Quaternion.LerpUnclamped(b.Source.parent.rotation, b.Source.parent.rotation * b.Target.rotation, x);
			});
		}
	}
}