using Core.Effects;
using Core.Entity.EntityAnimation;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Entity.Characters.Adapters
{
    public abstract class BaseCharacterAdapter : BaseLifeEntityAdapter<CharacterContext>
	{
		[TitleGroup("Base")]
		[ShowInInspector] [ReadOnly] protected CharacterContext _currentCharacterContext;
		[SerializeField] protected AdapterData AdapterData;

		
		private CharacterEffectAnimator _effectAnimator;
		private DamageEffectHandler _damageEffectHandler;

        public AimController AimController;
		public Vector3 Velocity => GetVel();

		private ReactiveCommand<CharacterContext> _contextChanged;
		public override IReactiveCommand<CharacterContext> ContextChanged => _contextChanged;
		public override CharacterContext CurrentContext => _currentCharacterContext;
		public abstract BaseEntityEffector Effector { get;}
		public override AnimatorAdapter AnimatorAdapter => CharacterAnimatorAdapter;
		public override Transform MainAdapterTransform => transform;
		public abstract CharacterAnimatorAdapter CharacterAnimatorAdapter { get; }

		public override void Init()
		{
			_contextChanged = new ReactiveCommand<CharacterContext>().AddTo(this);

			AimController.Init(this);
			Effector.Init();
			_effectAnimator = new CharacterEffectAnimator();
			_effectAnimator.Init(Effector);
			_damageEffectHandler = new DamageEffectHandler();
			_damageEffectHandler.Init(Effector);
        }
		
		public override void SetEntityContext(CharacterContext characterContext)
		{
			if (characterContext.IsOcupiedByAdapter)
			{
				Debug.LogError($"Try add adapter on ocupied context {name}[Adapter] {characterContext.name}[Context]");
				return;
			}
			_effectAnimator.SetContext(characterContext);
            _currentCharacterContext = characterContext;
            characterContext.SetAdapter(this);
			characterContext.IsOcupiedByAdapter = true;
			Effector.ChangeEffectsData(_currentCharacterContext.EffectsDataSo);
			_contextChanged.Execute(characterContext);
            _damageEffectHandler.SetContext(_currentCharacterContext.EffectsDataSo, _currentCharacterContext.Health, transform);
        }

		protected virtual Vector3 GetVel() => default;
		
		public virtual void OnContextDestroy()
		{
			BeforeDestroy();
			_currentCharacterContext = null;
			ContextChanged.Execute(null);
		}

		protected virtual void BeforeDestroy() {}
	}
}