using Core.Entity.Characters;
using System.Collections.Generic;

namespace Core.Effects
{
    public class CharacterEffectAnimator : BaseEffectAnimator
	{
		private CharacterContext _context;

		public void SetContext(CharacterContext context)
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
					_context.CharacterAnimator.SetShocked(isStarted);
					break;
                case EffectType.Fire:
                    _context.CharacterAnimator.SetFire(isStarted);
                    break;
            }
        }
	}
}
