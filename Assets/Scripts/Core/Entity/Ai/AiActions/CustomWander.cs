using Core.Entity.Ai.Movement;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Core.Entity.Ai.AiActions
{

	[Category("Movement/Pathfinding")]
	public class CustomWander : ActionTask<AiMovementController>
	{
		public bool Repeat = true;

		protected override void OnExecute() {
			DoWander();
		}
		
		protected override void OnUpdate() {
			if (agent.AgentStatusType == AgentStatusType.Waiting) {
				if ( Repeat && agent.enabled ) {
					DoWander();
				} else {
					EndAction();
				}
			}
		}
		
		private void DoWander()
		{
			agent.Wander();
		}

		protected override void OnPause()
		{
			OnStop();
		}
		
		protected override void OnStop() {
			agent.ResetPath();
		}
	}
}