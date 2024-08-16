using System.Collections.Generic;
using System.Threading;

namespace GameStateMachine.States.Impl.Project
{
	public class ProjectStateMachine : AbstractStateMachine
	{
		public override CancellationToken Token => CancellationToken.None;

		public ProjectStateMachine(IEnumerable<IGameState> gameStates) : base(gameStates)
		{
			
		}
	}
}