using Core.Entity.EntityAnimation;
using Core.Boosts.Impl;
using Core.HealthSystem;
using UniRx;
using UnityEngine;

namespace Core.Entity.Characters.Adapters
{
	public abstract class BaseLifeEntityAdapter<T> : MonoBehaviour, IEntityAdapter
		where T : LifeEntity
	{
		public abstract T CurrentContext { get; }
		public abstract AnimatorAdapter AnimatorAdapter { get; }
		public abstract IReactiveCommand<T> ContextChanged { get; }
		public abstract void SetEntityContext(T context);
		public abstract void Init();
		public abstract Transform MainAdapterTransform { get; }
		protected void OnContextSetup()
		{
			ContextChanged.Execute(CurrentContext);
		}
		
		public virtual void OnGetEffect(ref EffectArgs args) { }
		
		public virtual LifeEntity Entity => CurrentContext;
		public abstract IBoostProvider BoostProvider { get; }
		public abstract StatsProvider StatsProvider { get; }
	}

	public interface IEntityAdapter
	{
		LifeEntity Entity { get; }
		IBoostProvider BoostProvider { get; }
		StatsProvider StatsProvider { get; }
		AnimatorAdapter AnimatorAdapter { get; }
		Transform MainAdapterTransform { get; }
	}
}