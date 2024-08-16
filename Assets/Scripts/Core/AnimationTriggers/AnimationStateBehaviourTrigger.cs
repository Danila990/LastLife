using System;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Core.AnimationTriggers
{
	public abstract class AnimationStateBehaviourTrigger : ObservableStateMachineTrigger
	{
		[SerializeField] [Range(0, 1f)] protected float _normalizedTime;

		[NonSerialized][ShowInInspector] public float NormalizedTime;
		
		public abstract string AnimationTriggerKey { get; set; }
		protected ReactiveCommand<string> ReactiveCommand = new();
		public IObservable<string> Trigger => ReactiveCommand ??= new ReactiveCommand<string>();

		public void Awake()
		{
			NormalizedTime = _normalizedTime;
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfo.normalizedTime >= _normalizedTime)
				ReactiveCommand?.Execute(AnimationTriggerKey);
                
			base.OnStateUpdate(animator, stateInfo, layerIndex);
		}
	}
	
}