using System;

namespace Core.Entity.Ai
{
    [Flags]
    public enum Faction
    {
        None = 0,
        Shooter = 1 << 1,
        Heads = 1 << 2,
		
        All = ~(~0 << 3)
    }
}