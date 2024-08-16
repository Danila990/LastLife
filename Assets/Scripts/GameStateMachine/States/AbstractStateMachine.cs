using System;
using System.Collections.Generic;
using System.Threading;
using VContainer.Unity;

namespace GameStateMachine.States
{
    public abstract class AbstractStateMachine : IGameStateMachine, IInitializable
    {
        private readonly Dictionary<Type, IGameState> _allStates = new Dictionary<Type, IGameState>();
        private IGameState _curGameState;

        public abstract CancellationToken Token { get; }
        public IReadOnlyDictionary<Type, IGameState> AllStates => _allStates;
        
        public AbstractStateMachine(IEnumerable<IGameState> gameStates)
        {
            foreach (var state in gameStates)
            {
                _allStates[state.GetType()] = state;
            }
        }
        
        public void Initialize()
        {
            foreach (var state in _allStates.Values)
            {
                state.SetStateMachine(this);
            }
        }

        public void ChangeState<TState>() where TState : class, IGameState
        {
            var state = SwitchState<TState>();
            state.EnterState();
        }
        
        public void ChangeState(Type type)
        {
            _curGameState?.ExitState();
            _curGameState = _allStates[type];
            _curGameState.EnterState();
        }
        
        public void ChangeState<TPayload>(Type type, TPayload payload)
        {
            _curGameState?.ExitState();
            var state = (IPayloadGameState<TPayload>)_allStates[type];
            _curGameState = state;
            state.EnterState(payload);
        }

        public void ChangeStateAsync<TState>() where TState : class, IAsyncGameState
        {
            var state = SwitchState<TState>();
            state.EnterState(Token).Forget();
        }
        
        public void ChangeStateAsync<TState, TPayload>(TPayload payload) where TState : class, IPayloadAsyncGameState<TPayload>
        {
            var state = SwitchState<TState>();
            state.EnterState(Token, payload).Forget();
        }
        
        public void ChangeState<TState, TPayload>(TPayload payload) where TState : class, IPayloadGameState<TPayload>
        {
            var state = SwitchState<TState>();
            state.EnterState(payload);
        }

        private TState SwitchState<TState>() where TState : class, IGameState
        {
            _curGameState?.ExitState();
            var state = GetState<TState>();
            _curGameState = state;
            return state;
        }

        private TState GetState<TState>() where TState : class, IGameState
        {
            return _allStates[typeof(TState)] as TState;
        }
        
    }
}