using Core.Entity.Ai;

namespace Core.Entity
{
    public interface IFactionEntity : IEntity
    {
        Faction Faction { get; }
    }
}