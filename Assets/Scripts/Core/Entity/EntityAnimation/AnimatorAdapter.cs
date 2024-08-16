using UnityEngine;

namespace Core.Entity.EntityAnimation
{
	public abstract class AnimatorAdapter : MonoBehaviour
	{
		public abstract EntityAnimator EntityAnimator { get; }
		
		public abstract void OnContextSet<T>(EntityAnimator entityAnimator, T entityContext) where T : EntityContext, IControllableEntity;
		public abstract void Impact();
		public abstract void SetShocked(bool isShocked);
	}

	public enum AnimationType
	{
		fpv = 0,
		tpv = 1,
		all = 2
	}
}