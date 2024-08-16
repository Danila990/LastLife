using Core.Inventory.Items.Weapon;
using UnityEngine;
using Utils;

namespace Core.Actions.Impl
{
    [CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(RemoteDetonateAction), fileName = nameof(RemoteDetonateAction))]
    public class RemoteDetonateAction : GenericEntityAction<C4WeaponContext>
    {
        public override void OnDeselect()
        {
            if (CurrentContext)
                CurrentContext.Use(false);
        }
        public override void OnInput(bool state)
        {
            if (state && !CurrentContext.WeaponAdapter.InAttack)
            {
                CurrentContext.ExplodeRemote();
            }
        }
		
        public override void OnInputUp()
        {
			
        }
		
        public override void OnInputDown()
        {
        }
    }
}