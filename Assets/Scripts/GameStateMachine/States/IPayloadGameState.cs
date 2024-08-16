namespace GameStateMachine.States
{
    public interface IPayloadGameState<in TPayload> : IGameState
    {
        void EnterState(TPayload payload);
    }
}