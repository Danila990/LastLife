using System;
using Core.Entity.Characters;
using UnityEngine;
using Utils.Constants;

namespace Core.Effects
{
	public class SimpleEffectAnimator : BaseEffectAnimator
	{

		private readonly Animator _animator;

		public SimpleEffectAnimator(Animator animator)
		{
			_animator = animator;
		}

		protected override void OnEffect(EffectType effectType, bool isStarted)
		{
			switch (effectType)
			{
				case EffectType.None:
					break;
				case EffectType.Electric:
					_animator.SetBool(AHash.Shocked, isStarted);
					break;
			}
		}
	}
}
