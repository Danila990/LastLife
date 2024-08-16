using UnityEngine;

namespace StateMachine
{
    public interface IState
    {
        public bool CanExit { get; }

        public void Enter();
        public void Exit();
        public void HandleInput();
        public void Update();
        public void PhysicsUpdate();
        public void LateUpdate();
        public void OnTriggerEnter(Collider collider);
        public void OnTriggerExit(Collider collider);
        public void OnAnimationEnterEvent();
        public void OnAnimationExitEvent();
        public void OnAnimationTriggerEvent(string key);
        void OnLoseBalance();
        void OnRegainBalance();
        void UseAction();
    }
}