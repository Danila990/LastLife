using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl.StaticItemActions
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(StaticItemShoot), fileName = "StaticItemShoot")]
	public class StaticItemShoot : GenericEntityAction<ProjectileStaticWeaponContext>
	{
		[Inject] private readonly IRayCastService _rayCastService;

		public override void OnDeselect()
		{
		}
		
		public override void OnInput(bool state)
		{
			 
		}

		public override void OnInputUp()
		{
			CurrentContext.SetShootStatus(false);
		}
		
		public override void OnInputDown()
		{
			CurrentContext.SetShootStatus(true);
		}
	}
}