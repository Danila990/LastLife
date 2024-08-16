using Core.Entity;
using Core.Entity.Characters;
using UnityEngine;
using Utils;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(AimAction), fileName = nameof(AimAction))]
	public class AimAction : ItemEntityAction
	{
		public CharacterContext Owner { get; set; }

		public override void OnDeselect()
		{
			CurrentContext.TryAimState(false);
		}
		
		public override void OnInput(bool state)
		{
			
		}
		
		public override void OnInputUp()
		{
		}
		
		public override void OnInputDown()
		{
			CurrentContext.TryAimState(!Owner.CurrentAdapter.AimController.IsAiming.Value);
		}
		
		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);
			Owner = CurrentContext.Owner as CharacterContext;
		}
	}
}