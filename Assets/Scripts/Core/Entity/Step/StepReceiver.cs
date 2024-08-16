using FIMSpace.FProceduralAnimation;
using UnityEngine;
using Utils;

namespace Core.Entity.Step
{
	public class StepReceiver : MonoBehaviour, LegsAnimator.ILegStepReceiver
	{
		[SerializeField] private FootstepData _footstepData;
		[SerializeField] private EntityContext _context;

		public void LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type)
		{
			if (!Physics.Raycast(position + Vector3.up * 0.25f, Vector3.down, out var hit, 1f, LayerMasks.Environment))
				return;
			
			var footstep = _footstepData.GetClip(hit.collider.sharedMaterial);
			_context.AudioService
					.PlayNonQueue(footstep.Clip)
					.SetPosition(position)
					.SetVolume(footstep.Settings.Volume)
					.SetSpatialBlend(1f)
					.SetSpread(footstep.Settings.Spread)
					.SetMaxDistance(footstep.Settings.MaxDistance)
					.SetPitch(footstep.Settings.Pitch)
					.SetRolloffMode(footstep.Settings.Mode);
		}

	}
}
