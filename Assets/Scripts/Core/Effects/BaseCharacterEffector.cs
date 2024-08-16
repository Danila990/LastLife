using System;
using Core.Boosts.Impl;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using UniRx;
using UnityEngine;

namespace Core.Effects
{

	public abstract class BaseCharacterEffector : LifeEntityEffector
	{
		protected CharacterContext CurrentContext;
		protected IEntityAdapter Adapter;

		private CompositeDisposable _disposable;
		
		public override void SetContext(LifeEntity context)
		{
            base.SetContext(context);
            CurrentContext = (CharacterContext) context;
			Adapter = CurrentContext.Adapter;

			_disposable?.Dispose();
			if (Adapter.BoostProvider != null)
			{
				_disposable = new CompositeDisposable();
				Adapter.BoostProvider.OnBoostApplied.Subscribe(OnBoostApplied).AddTo(_disposable);
				Adapter.BoostProvider.OnBoostEnded.Subscribe(OnBoostRemoved).AddTo(_disposable);
			}
		}

		protected virtual void OnBoostApplied(AppliedBoostArgs appliedBoostArgs) { }
		protected virtual void OnBoostRemoved(BoostArgs boostArgs) { }

		public override void OnDisable()
		{
			base.OnDisable();
			_disposable?.Dispose();
		}
	}

}
