using UnityEngine;

namespace Core.AnimationTriggers
{
	public class AttackStateBehTrigger : AnimationStateBehaviourTrigger
	{
		private bool _wasUsed = false;
		[field:SerializeField] public override string AnimationTriggerKey { get; set; }

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);
			_wasUsed = false;
		}
		
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfo.normalizedTime/stateInfo.speed >= NormalizedTime && _wasUsed == false)
			{
				ReactiveCommand?.Execute(AnimationTriggerKey);
				_wasUsed = true;
			}
		}
	}
}