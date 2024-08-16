using System;
using Core.AnimationRigging;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Utils.Constants;

namespace Core.Entity.Ai.Npc
{
	public class NpcAnimator : MonoBehaviour
	{
		[field:SerializeField] public Animator Animator { get; private set; }
		[field:SerializeField] public NavMeshAgent NavMeshAgent { get; private set; }
		[field:SerializeField] public MonoRigProvider MonoRigProvider { get; private set; }
		
		private IDisposable _trackDisposable;

		private void Update()
		{
			var magnitude = NavMeshAgent.velocity.magnitude;
			Animator.SetBool(AHash.MovingParameterHash, magnitude > 0.01f);
			Animator.SetFloat(AHash.Speed, magnitude / NavMeshAgent.speed);
		}

		public void PlayAnimation(string animationState)
		{
			Animator.Play(animationState);
		}

		public void Impact()
		{
			Animator.SetTrigger(AHash.Impact);
		}

		public void TrackObject(Transform objectToTrack)
		{
			_trackDisposable?.Dispose();
			if (MonoRigProvider.Rigs.TryGetValue("Head", out var rig))
			{
				rig.EnableRig();
				_trackDisposable = Observable.EveryUpdate()
					.TakeUntilDestroy(objectToTrack)
					.Finally(FinallyAction)
					.SubscribeWithState2(objectToTrack, rig, MoveLeftToRight);
			}
		}
		
		private void FinallyAction()
		{
			if (MonoRigProvider.Rigs.TryGetValue("Head", out var rig))
			{
				rig.DisableRig();
			}
		}

		public void StopTrack()
		{
			_trackDisposable?.Dispose();
		}
		
		private void MoveLeftToRight(long arg1, Transform t1, RigElementController rig)
		{
			rig.RigTarget.position = t1.position;
		}
	}
}