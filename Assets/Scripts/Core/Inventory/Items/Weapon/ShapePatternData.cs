using UnityEngine;
using Utils;

namespace Core.Inventory.Items.Weapon
{
    [CreateAssetMenu(menuName = SoNames.SHOOT_PATTERN + nameof(ShapePatternData), fileName = nameof(ShapePatternData))]
    public class ShapePatternData : PatternDataSO
    {
        [SerializeField] private ShapeShootPattern _pattern;
        public override ShootPattern GetPattern()
        {
            return _pattern;
        }
    }
}