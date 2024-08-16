using UnityEngine;

namespace Core.Quests.Tips.Impl.Interfaces
{
	public interface ITrackedTip
	{
		public void SetTrackedTarget(Transform target, Vector3 offset, Vector3 eulerAngels);
	}
}
