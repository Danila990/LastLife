using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.AnimationTriggers;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utils;

namespace Core.Entity.Characters
{
    public class AnimatorObserver : IDisposable
    {
        private readonly Animator _animator;
        private IObservable<string> _actionObservable;
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly CancellationTokenSource _cts = new();
        public event Action<string> AnimationTrigger;
        public event Action<ObservableStateMachineTrigger.OnStateInfo> AnimationTriggerExit;
        public event Action<ObservableStateMachineTrigger.OnStateInfo> AnimationTriggerEnter;
        
        public AnimatorObserver(
            Animator animator
        )
        {
            _animator = animator;
        }


        private bool TryGetBehaviour(string key,out AnimationStateBehaviourTrigger behaviourTrigger)
        {
            var behaviours = _animator.GetBehaviours<AnimationStateBehaviourTrigger>();
            var find = behaviours.FirstOrDefault(x => x.AnimationTriggerKey.Equals(key));
            if (find)
            {
                behaviourTrigger = find;
                return true;
            }
            behaviourTrigger = default;
            return false;
        }

        public bool HasBehaviour(string key)
        {
            var behaviours = _animator.GetBehaviours<AnimationStateBehaviourTrigger>();
            var find = behaviours.FirstOrDefault(x => x.AnimationTriggerKey.Equals(key));
            return find;
        }

        public async UniTask<(bool status,string result)> TryAwaitTrigger(string hashKey, float timeout)
        {
            if (TryGetBehaviour(hashKey,out var eventData))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
                var callback = await eventData.Trigger.ToUniTask(true, cts.Token)
                    .TimeoutWithoutException(timeout.ToSec(), taskCancellationTokenSource: cts);
                if (!cts.IsCancellationRequested)
                {
                    cts?.Cancel();
                    cts?.Dispose();
                }
                return (!callback.IsTimeout, callback.Result);
            }
            return (false,default);
        }

        private void OnStateEnter(ObservableStateMachineTrigger.OnStateInfo obj)
        {
            AnimationTriggerEnter?.Invoke(obj);
        }

        private void OnStateExit(ObservableStateMachineTrigger.OnStateInfo obj)
        {
            AnimationTriggerExit?.Invoke(obj);
        }
		
        private void OnTrigger(string key)
        {
            AnimationTrigger?.Invoke(key);
        }
        private void RemoveHandler(Action<string> obj)
        {
            AnimationTrigger -= obj;
        }
		
        private void Handler(Action<string> obj)
        {
            AnimationTrigger += obj;
        }
        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            if(_cts.IsCancellationRequested) 
                return;
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}