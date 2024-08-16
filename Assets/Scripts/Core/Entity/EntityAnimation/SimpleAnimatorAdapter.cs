
namespace Core.Entity.EntityAnimation
{
	public class SimpleAnimatorAdapter : AnimatorAdapter
	{
		private EntityAnimator _entityAnimator;
		public override EntityAnimator EntityAnimator => _entityAnimator;

		public override void OnContextSet<T>(EntityAnimator entityAnimator, T entityContext)
		{
			_entityAnimator = entityAnimator;
		}
		
		public override void Impact() => _entityAnimator.Impact();
		public override void SetShocked(bool isShocked) => _entityAnimator.SetShocked(isShocked);
	}
}