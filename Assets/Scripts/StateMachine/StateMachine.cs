using UnityEngine;

namespace StateMachine
{
    public abstract class StateMachine
    {
        protected IState CurrentState;

        public IState State => CurrentState;

        public void ChangeState(IState newState)
        {
            if(CurrentState is { CanExit: false })
                return;
            CurrentState?.Exit();

            CurrentState = newState;
            CurrentState.Enter();
        }

        public void HandleInput()
        {
            CurrentState?.HandleInput();
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void PhysicsUpdate()
        {
            CurrentState?.PhysicsUpdate();
        }

        public void LateTick()
        {
            CurrentState?.LateUpdate();
        }

        public void OnTriggerEnter(Collider collider)
        {
            CurrentState?.OnTriggerEnter(collider);
        }

        public void OnTriggerExit(Collider collider)
        {
            CurrentState?.OnTriggerExit(collider);
        }

        public void OnAnimationEnterEvent()
        {
            CurrentState?.OnAnimationEnterEvent();
        }

        public void OnAnimationExitEvent()
        {
            CurrentState?.OnAnimationExitEvent();
        }

        public void OnAnimationTriggerEvent(string triggerKey)
        {
            CurrentState?.OnAnimationTriggerEvent(triggerKey);
        }
    }
}