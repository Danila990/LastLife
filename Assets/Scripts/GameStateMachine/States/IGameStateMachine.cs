
using System;

namespace GameStateMachine.States
{
    public interface IGameStateMachine
    {
        void ChangeState<TState>() where TState : class, IGameState;
        void ChangeState(Type type);
        void ChangeState<TPayload>(Type type, TPayload payload);
        void ChangeStateAsync<TState>() where TState : class, IAsyncGameState;
        void ChangeState<TState, TPayload>(TPayload payload) where TState : class, IPayloadGameState<TPayload>;
        void ChangeStateAsync<TState, TPayload>(TPayload payload) where TState : class, IPayloadAsyncGameState<TPayload>;
    }
}