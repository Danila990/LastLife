using Core.Boosts.Entity;
using UnityEngine;
using Utils;

namespace Core.Actions
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(UseBoostAction), fileName = nameof(UseBoostAction))]
	public class UseBoostAction : GenericEntityAction<BoostEntity>
	{
		public override void OnDeselect()
		{
		}
		
		public override void OnInput(bool state)
		{
		}
		
		public override void OnInputUp()
		{
		}
		
		public override void OnInputDown()
		{
			CurrentContext.ApplyBoostAnimated();
		}
	}

}