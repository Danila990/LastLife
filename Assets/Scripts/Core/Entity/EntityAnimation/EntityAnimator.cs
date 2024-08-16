using UnityEngine;

namespace Core.Entity.EntityAnimation
{
	public abstract class EntityAnimator : MonoBehaviour
	{
		public abstract void Impact();
		public abstract void SetShocked(bool isShocked);
	}
}