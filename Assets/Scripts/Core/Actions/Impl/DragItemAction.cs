using Core.Entity.InteractionLogic;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(DragItemAction), fileName = nameof(DragItemAction))]
	public class DragItemAction : ItemEntityAction
	{
		[Inject] private readonly IDragInteractionService _dragInteractionService;

		public override void OnDeselect()
		{
			_dragInteractionService.OnInput(false);
		}
		
		public override void OnInput(bool state)
		{
			_dragInteractionService.OnInput(state);
		}

		public override void OnInputUp()
		{
			
		}
		
		public override void OnInputDown()
		{
			
		}
	}
}