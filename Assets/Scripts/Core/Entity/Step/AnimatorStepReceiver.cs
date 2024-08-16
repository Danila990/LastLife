using UnityEngine;
using Utils;

namespace Core.Entity.Step
{
	public class AnimatorStepReceiver : MonoBehaviour
	{
		[SerializeField] private FootstepData _footstepData;
		[SerializeField] private EntityContext _context;

		public void OnStep()
		{
			if (!Physics.Raycast(transform.position, Vector3.down, out var hit, 1f, LayerMasks.Environment))
				return;
			var footstep = _footstepData.GetClip(hit.collider.sharedMaterial);
			if (_context.AudioService.TryPlayQueueSound(footstep.Clip, _context.Uid.ToString(), 0.1f,
				    out var player))
			{
				player
					.SetPosition(transform.position)
					.SetVolume(footstep.Settings.Volume)
					.SetSpatialBlend(1f)
					.SetSpread(footstep.Settings.Spread)
					.SetMaxDistance(footstep.Settings.MaxDistance)
					.SetPitch(footstep.Settings.Pitch)
					.SetRolloffMode(footstep.Settings.Mode);
			}
		}
	}
}
