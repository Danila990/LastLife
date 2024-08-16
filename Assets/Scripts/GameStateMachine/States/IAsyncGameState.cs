using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameStateMachine.States
{
	public interface IAsyncGameState : IGameState
	{
		UniTaskVoid EnterState(CancellationToken token);
	}
	
	public interface IPayloadAsyncGameState<in TPayload> : IGameState
	{
		UniTaskVoid EnterState(CancellationToken token, TPayload payload);
	}
}