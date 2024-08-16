using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Characters;
using Core.Factory.VFXFactory;
using Core.HealthSystem;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Effects
{
	public abstract class BaseEntityEffector : MonoBehaviour
	{
		[field: SerializeField] public EffectsDataSo EffectsDataSo { get; protected set; }

		[Inject] protected IVFXFactory VfxFactory;

		protected ReactiveCommand<EffectType> _onEffectStart;
		protected ReactiveCommand<EffectType> _onEffectEnd;

		protected readonly Dictionary<string, Effect> _activeEffects = new Dictionary<string, Effect>();
		protected readonly List<Effect> _effects = new List<Effect>();

		public bool IsEffected => _activeEffects.Count > 0;
		public IReactiveCommand<EffectType> OnEffectStart => _onEffectStart;
		public IReactiveCommand<EffectType> OnEffectEnd => _onEffectEnd;

		protected static (string, Effect) Empty => (string.Empty, null);

		public void Init()
		{
			_onEffectStart = new ReactiveCommand<EffectType>();
			_onEffectEnd = new ReactiveCommand<EffectType>();
		}
		
		public void SetFxFactory(IVFXFactory factory)
		{
			VfxFactory = factory;
		}

		public void ChangeEffectsData(EffectsDataSo dataSo)
		{
			if(!dataSo)
				return;
			
			EffectsDataSo = dataSo;
		}

		public virtual void DoEffect(EffectArgs args)
		{
			if (_activeEffects.TryGetValue(args.EffectType.ToString(), out var timer))
			{
				timer.Renew();
				return;
			}

			var effect = DoEffectInternal(args);

			if (effect != Empty)
			{
				_activeEffects.Add(effect.Item1, effect.Item2);
				_effects.Add(effect.Item2);
				_onEffectStart.Execute(args.EffectType);
			}
		}
		
		protected void  RemoveEffect(string type)
		{
			if (_activeEffects.TryGetValue(type, out var effect))
			{
				effect.Dispose();
				_onEffectEnd.Execute(effect.EffectType);
				_activeEffects.Remove(type);
				_effects.Remove(effect);
			}
		}

		public void RemoveAllEffects()
		{
			while (_effects.Count > 0)
			{
				var effect = _effects[0];
				if(effect == null)
				{
					_effects.RemoveAt(0);
					continue;
				}
				
				
				effect.ForceStop();
				_onEffectEnd?.Execute(effect.EffectType);
				if (_effects.Count > 0 && effect == _effects[0])
				{
					_effects.RemoveAt(0);
				}
			}
			
			_activeEffects.Clear();
			_effects.Clear();
		}
		
		protected abstract (string, Effect) DoEffectInternal(EffectArgs args);

		public virtual void OnDisable()
		{
			foreach (var effect in _activeEffects.Values)
				effect?.Dispose();
			
			_onEffectStart?.Dispose();
			_onEffectEnd?.Dispose();
		}
	}

}
