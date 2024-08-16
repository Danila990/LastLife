namespace GameStateMachine.States
{
    public interface IGameState
    {
        void EnterState();
        void ExitState();
        void SetStateMachine(IGameStateMachine stateMachine);
    }
}