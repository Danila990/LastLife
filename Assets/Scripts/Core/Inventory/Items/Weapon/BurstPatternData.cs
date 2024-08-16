using UnityEngine;
using Utils;

namespace Core.Inventory.Items.Weapon
{
    [CreateAssetMenu(menuName = SoNames.SHOOT_PATTERN + nameof(BurstPatternData), fileName = nameof(BurstPatternData))]
    public class BurstPatternData : PatternDataSO
    {
        [SerializeField] private BurstShootPattern _pattern;
        public override ShootPattern GetPattern()
        {
            return _pattern;
        }
    }
}