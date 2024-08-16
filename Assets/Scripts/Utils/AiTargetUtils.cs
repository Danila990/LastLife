using Core.Entity;
using Core.Entity.Ai;

namespace Utils
{
	public static class AiTargetUtils
	{
		public static bool TryGetEntity(this IAiTarget aiTarget, out LifeEntity lifeEntity)
		{
			if (aiTarget is EntityTarget entityTarget)
			{
				lifeEntity = entityTarget.Entity;
				return true;
			}
			lifeEntity = null;
			return false;
		}
	}
}