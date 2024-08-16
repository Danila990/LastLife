using Core.Entity.Head;
using NodeCanvas.Framework;

namespace Core.Entity.Ai.AiActions
{
	public class SetRotationStatusAction : ActionTask<AiJetHeadAdapter>
	{
		public BBParameter<bool> Status;

		protected override void OnExecute()
		{
			if (!agent.MovementController)
			{
				EndAction();
				return;
			}


			agent.MovementController.IsRotateByPath = Status.value;
			EndAction();
		}
	}
}
