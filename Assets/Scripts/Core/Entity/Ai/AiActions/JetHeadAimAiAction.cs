using Core.Entity.Ai.AiItem;
using Core.Entity.Ai.Movement;
using NodeCanvas.Framework;

namespace Core.Entity.Ai.AiActions
{
	public class JetHeadAimAiAction : ActionTask<FlyingAiMovementController>
	{
		public BBParameter<IAiTarget> Target;
		public BBParameter<IAiItem> Item;
		private IAiItem _item;

		protected override void OnExecute()
		{
			_item = Item.value;
			
			if (_item is null)
			{
				EndAction(false);
			}
		}
		
		protected override void OnUpdate()
		{
			if (!Target.value.IsActive)
			{
				EndAction(false);
				return;
			}
			
			if (_item != null && (elapsedTime >= _item.UseActionDuration || !_item.InUse)) {
				EndAction();
				return;
			}
		}
	}
}