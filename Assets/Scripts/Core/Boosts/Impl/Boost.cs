using System;
using UniRx;
using UnityEngine;

namespace Core.Boosts.Impl
{
	public abstract class Boost : IBoost, IBoostImpl, IDisposable 
	{
		private readonly ReactiveCommand<string> _onApply;
		private readonly ReactiveCommand<string> _onRemove;
		
		protected readonly BoostArgs BoostArgs;

		public abstract BoostCategory Category { get; }
		public abstract string AdditionalName { get; }
		public bool IsApplied { get; set; }

		public IReactiveCommand<string> OnApply => _onApply;
		public IReactiveCommand<string> OnRemove => _onRemove;
		BoostArgs IBoost.BoostArgs => BoostArgs;

		private bool CompareCategory(BoostCategory category)
		{
			return Category == category;
		}
		
		protected Boost(BoostArgs boostArgs)
		{
			if (!CompareCategory(boostArgs.Category))
			{
				Debug.LogError($"Invalid boost. This handler({this}) cannot handle boost({boostArgs.Type})");
				return;
			}
			
			BoostArgs = boostArgs;
			_onApply = new ReactiveCommand<string>();
			_onRemove = new ReactiveCommand<string>();
		}
		
		public void ApplyEffect()
		{
			if(IsApplied)
				return;
			
			IsApplied = true;
			ApplyEffectInternal();
			_onApply?.Execute(BoostArgs.Type);
		}
		
		public void RemoveEffect()
		{
			if(!IsApplied)
				return;
			
			IsApplied = false;
			RemoveEffectInternal();
			_onRemove?.Execute(BoostArgs.Type);
			Dispose();
		}

		protected abstract void ApplyEffectInternal();
		protected abstract void RemoveEffectInternal();

		public virtual void Dispose()
		{
			_onApply?.Dispose();
			_onRemove?.Dispose();
		}
	}
}
