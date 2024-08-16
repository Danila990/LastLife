using Core.Effects;
using Core.Entity.Head;
using Core.HealthSystem;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Entity.Characters.Adapters
{
	public abstract class BaseHeadAdapter : BaseLifeEntityAdapter<HeadContext>
	{
		public LifeEntityEffector Effector;
		
		[TitleGroup("Base")]
		[ShowInInspector] [ReadOnly] private HeadContext _currentHeadContext;
		
		private ReactiveCommand<HeadContext> _contextChanged;
		private HeadEffectAnimator _effectAnimator;
		public abstract IRagdollManager RagdollManager { get; }
		public override IReactiveCommand<HeadContext> ContextChanged => _contextChanged;

		public override HeadContext CurrentContext => _currentHeadContext;
		
		public override void Init()
		{
			_contextChanged = new ReactiveCommand<HeadContext>().AddTo(this);
			Effector.Init();
			_effectAnimator = new();
			_effectAnimator.Init(Effector);
		}
		
		public override void SetEntityContext(HeadContext context)
		{
			_effectAnimator.SetContext(context);
			_currentHeadContext = context;
			_currentHeadContext.CurrentAdapter = this;
			Effector.SetContext(context);
			Effector.SetFxFactory(Entity.VFXFactory);

		}
		
		public virtual void DisableControl()
		{
			
		}
		
		public virtual void EnableControl()
		{
			
		}

		public void OnDestroyed()
		{
		}
		
		public virtual void OnDied()
		{
			
		}
		public abstract void OnBossRoomOpened();

		public override void OnGetEffect(ref EffectArgs args)
		{
			Effector.DoEffect(args);
		}
	}
}