using Core.Inventory.Items.Weapon;
using UnityEngine;
using Utils;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(MeleeLegKickAction), fileName = nameof(MeleeLegKickAction))]
	public class MeleeLegKickAction : GenericEntityAction<MeleeWeaponContext>, IAnimationEntityAction
	{
		public AnimationClip Clip => null;

		public void UseFromEvent()
		{
			//CurrentContext.Kick();
		}

		public override void OnDeselect(){ }
		public override void OnInput(bool state) { }
		public override void OnInputUp() { }
		
		public override void OnInputDown()
		{
			CurrentContext.UseLegAttack(this);	
		}
	}
}