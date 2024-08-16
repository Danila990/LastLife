using Core.Boosts.Entity;
using Core.Entity;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Actions
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(SwitchBoostAction), fileName = nameof(SwitchBoostAction))]
	public class SwitchBoostAction : GenericEntityAction<BoostEntity>, IEnableAction
	{
		public IReactiveProperty<bool> IsEnabled { get; set; }
		
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
			CurrentContext.SwitchBoost();
		}

		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);
			IsEnabled = CurrentContext.CanSwitch;
		}
	}
}