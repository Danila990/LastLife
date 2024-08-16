using System;
using Core.Entity.Characters;
using Core.Entity.Head;
using Utils.Constants;

namespace Core.Effects
{
	public class HeadEffectAnimator : BaseEffectAnimator
	{
		private HeadContext _context;

		public void SetContext(HeadContext context)
		{
			_context = context;
		}
		
		protected override void OnEffect(EffectType effectType, bool isStarted)
		{
			if (!_context || _context.Health.IsDeath)
				return;
			
			switch (effectType)
			{
				case EffectType.None:
					break;
				case EffectType.Electric:
					_context.Animator.SetBool(AHash.Shocked,isStarted);
					break;
			}

		}
	}
}
