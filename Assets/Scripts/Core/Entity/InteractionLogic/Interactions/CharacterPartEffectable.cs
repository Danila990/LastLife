using Core.Entity.Characters;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class CharacterPartEffectable : EntityEffectable, ICharacterContextAcceptor
	{
		[SerializeField] private CharacterContext _characterContext;

		public void SetContext(CharacterContext context)
		{
			_characterContext = context;
		}

	}
}
