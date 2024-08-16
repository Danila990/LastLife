using Core.Inventory.Items.Weapon;
using UnityEngine;
using Utils;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(MeleePunchAction), fileName = nameof(MeleePunchAction))]
	public class MeleePunchAction : GenericEntityAction<MeleeWeaponContext>
	{
		public override void OnDeselect()
		{
			if (CurrentContext)
				CurrentContext.UseSimpleAttack(false);	
		}
		
		public override void OnInput(bool state)
		{
			CurrentContext.UseSimpleAttack(state);	
		}
		
		public override void OnInputUp()
		{
			
		}
		
		public override void OnInputDown()
		{
			
		}
	}

}