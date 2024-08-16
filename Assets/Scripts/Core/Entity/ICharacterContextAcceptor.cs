using Core.Entity.Characters;

namespace Core.Entity
{
    public interface ICharacterContextAcceptor
    {
        public void SetContext(CharacterContext context);
    }
    
    public interface IContextAcceptor
    {
        public void SetContext(EntityContext context);
    }
}