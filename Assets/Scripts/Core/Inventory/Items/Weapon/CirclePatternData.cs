using UnityEngine;
using Utils;

namespace Core.Inventory.Items.Weapon
{
    [CreateAssetMenu(menuName = SoNames.SHOOT_PATTERN + nameof(CirclePatternData), fileName = nameof(CirclePatternData))]
    public class CirclePatternData : PatternDataSO
    {
        [SerializeField] private CircleShootPatter _pattern;
        public override ShootPattern GetPattern()
        {
            return _pattern;
        }
    }
}