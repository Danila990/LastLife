using System;
using System.Collections.Generic;
using System.Linq;
using Core.AnimationTriggers;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utils.Constants;

namespace Core.Entity.EntityAnimation
{
	public class CharacterAnimator : EntityAnimator
	{
		public Animator Animator;
		public AnimationClip ClipToOverride;
		
		private IObservable<string> _actionObservable;
		private AnimatorOverrideController _overrideController;
		private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overrides = new();

		private bool _needToObserve;
		
		public event Action<string> AnimationTrigger;
		public event Action<ObservableStateMachineTrigger.OnStateInfo> AnimationTriggerExit;
		public event Action<ObservableStateMachineTrigger.OnStateInfo> AnimationTriggerEnter;

		private void Awake()
		{
			_actionObservable = Observable.FromEvent<string>(Handler, RemoveHandler);
			_overrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
			Animator.runtimeAnimatorController = _overrideController;
			_overrideController.GetOverrides(_overrides);
		}
		
		public void ObserveAnimationEvents()
		{
			_needToObserve = true;
			var behaviours = Animator.GetBehaviours<AnimationStateBehaviourTrigger>();

			foreach (var behaviour in behaviours)
			{
				behaviour.OnStateExitAsObservable().Subscribe(OnStateExit).AddTo(gameObject);
				behaviour.OnStateEnterAsObservable().Subscribe(OnStateEnter).AddTo(gameObject);
				behaviour.Trigger.Subscribe(OnTrigger).AddTo(gameObject);
			}
		}

		public async UniTask<bool> PlayAction(AnimationClip clip, float triggerTime, float awaitingTime = 2f, float speedMul = 1f)
		{
			OverrideClip(clip);
			var beh = Animator
				.GetBehaviours<AnimationStateBehaviourTrigger>()
				.First(x=> x.AnimationTriggerKey.Equals("SwapAction"));
			
			beh.NormalizedTime = triggerTime;
			Animator.Play("Action");
			Animator.SetFloat(AHash.ActionMultiplier, speedMul);
			
			if (!_needToObserve)
			{
				await UniTask.Delay(triggerTime.ToSec(), cancellationToken: destroyCancellationToken);
				Animator.SetFloat(AHash.ActionMultiplier, 1f);
				return true;
			}
			
			var eventObject = await _actionObservable
				.ToUniTask(cancellationToken: destroyCancellationToken, useFirstValue: true)
				.TimeoutWithoutException(awaitingTime.ToSec());

			if (destroyCancellationToken.IsCancellationRequested)
				return false;
			
			Animator.SetFloat(AHash.ActionMultiplier, 1f);

			return !eventObject.IsTimeout;
		}

		private void OverrideClip(AnimationClip clip)
		{
			for (var index = 0; index < _overrides.Count; index++)
			{
				if (_overrides[index].Key.Equals(ClipToOverride))
				{
					_overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(ClipToOverride, clip);
					_overrideController.ApplyOverrides(_overrides);
					break;
				}
			}
		}
		
		public override void Impact()
		{
			Animator.SetTrigger(AHash.Impact);
		}
		
		public override void SetShocked(bool isShocked)
		{
			Animator.SetBool(AHash.Shocked, isShocked);
		}

        public void SetFire(bool isFire)
        {
            Animator.SetBool(AHash.Fire, isFire);
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
	}
}