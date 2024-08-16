using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public abstract class PatternDataSO : ScriptableObject
    {
        public abstract ShootPattern GetPattern();
    }
}