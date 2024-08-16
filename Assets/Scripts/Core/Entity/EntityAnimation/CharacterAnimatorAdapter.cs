using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Entity.EntityAnimation
{
	public class CharacterAnimatorAdapter : AnimatorAdapter
	{
		protected CharacterAnimator CharacterAnimator { get; private set; }
		public override EntityAnimator EntityAnimator => CharacterAnimator;

		public override void OnContextSet<T>(EntityAnimator entityAnimator, T entityContext)
		{
			CharacterAnimator = entityAnimator as CharacterAnimator;
			Debug.Assert(CharacterAnimator);
		}
		
		public override void Impact()
		{
			CharacterAnimator.Impact();
		}
		
		public override void SetShocked(bool isShocked)
		{
			CharacterAnimator.SetShocked(isShocked);
		}

		public UniTask<bool> PlayAction(AnimationClip clip, float triggerTime, float awaitingTime = 2, float speedMul = 1)
		{
			return CharacterAnimator.PlayAction(clip, triggerTime, awaitingTime, speedMul);
		}
		
		public virtual UniTask<bool> PlayAction(
			AnimationClip clip,
			string fpvAnimKey, 
			string fpvEventKey,
			float triggerTime, 
			float awaitingTime = 2f,
			float speedMul = 1)
		{
			return CharacterAnimator.PlayAction(clip, triggerTime, awaitingTime, speedMul);
		}
		
		public virtual UniTask<bool> LegAttack(
			AnimationClip clip,
			string fpvAnimKey, 
			string fpvEventKey,
			float triggerTime, 
			Action callback,
			float awaitingTime = 2,
			float speedMul = 1)
		{
			return CharacterAnimator.PlayAction(clip, triggerTime, awaitingTime, speedMul);
		}
		
		public virtual void SetFloat(int hash, float value)
		{
			CharacterAnimator.Animator.SetFloat(hash, value);
		}
		
		public virtual void Play(int hash, AnimationType type)
		{
			CharacterAnimator.Animator.Play(hash);
		}
	}
}