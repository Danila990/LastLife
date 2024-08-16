
using UnityEngine;

namespace Core.Services.Experience
{
	public readonly struct ExperienceMessage
	{
		public readonly float ExperienceCount;
		public readonly object SourceFrom;
		public readonly object TargetToAdd;
		public readonly bool IsForce;
		public readonly bool SkipAnim;
		public readonly Vector3 Scale;
		public readonly Vector3 Position;
		
		public ExperienceMessage(
			float experienceCount,
			object source, 
			object targetToAdd, 
			Vector3 position,
			float scale = 1f)
		{
			ExperienceCount = experienceCount;
			SourceFrom = source;
			TargetToAdd = targetToAdd;
			Scale = Vector3.one * scale;
			Position = position;
			IsForce = false;
			SkipAnim = false;
		}

		public ExperienceMessage(bool isForce = true, float experienceCount = 0, bool skipAnim = true)
		{
			ExperienceCount = experienceCount;
			SourceFrom = null;
			TargetToAdd = null;
			IsForce = isForce;
			Scale = Vector3.one;
			Position = default;
			SkipAnim = skipAnim;
		}
	}
}