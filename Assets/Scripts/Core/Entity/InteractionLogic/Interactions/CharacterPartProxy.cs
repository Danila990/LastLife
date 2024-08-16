
using Core.Entity.Characters;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class CharacterPartProxy : MonoInteractProvider
    {
        private CharacterContext _characterContext;
        
        protected override void SetContext(IInteractableContexted inter)
        {
            inter.SetCharContext(_characterContext ??= _context as CharacterContext);
        }
    }
}