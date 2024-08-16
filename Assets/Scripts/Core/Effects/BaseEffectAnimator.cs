using Core.Entity.Characters;
using UniRx;

namespace Core.Effects
{
	public abstract class BaseEffectAnimator
	{
		public void Init(BaseEntityEffector effector)
		{
			effector.OnEffectStart.SubscribeWithState(true, OnEffect).AddTo(effector);
			effector.OnEffectEnd.SubscribeWithState(false, OnEffect).AddTo(effector);
		}

		protected abstract void OnEffect(EffectType effectType, bool isStarted);
	}

}
