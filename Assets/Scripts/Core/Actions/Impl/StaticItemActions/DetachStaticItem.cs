using Core.Entity.InteractionLogic.Interactions;
using Core.Services;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl.StaticItemActions
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(DetachStaticItem), fileName = "DetachStaticItem")]
	public class DetachStaticItem : GenericEntityAction<StaticItemContext>
	{
		[Inject] private readonly IPlayerStaticInteractionService _playerInteractionService;
		
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
			_playerInteractionService.Detach();
		}
	}
}