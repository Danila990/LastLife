using System;
using System.Linq;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Common
{
	public class MotionAnimator : MonoBehaviour
	{
		public Transform[] Targets;

		public bool Shake;
		public bool InfinityLoops;
		public LoopType LoopType;
		public Ease Easing;
		public Vector3 Strength;
		public float Duration;

		[PropertyRange(0, 1)]
		public float RandomDelay;
		
		private CompositeMotionHandle _compositeMotionHandle;
		private Vector3[] _savedLocalPos;

		private void OnDisable()
		{
			_compositeMotionHandle?.Cancel();
		}

		[Button]
		public void EnableAnim()
		{
			DisableAnim();
			_compositeMotionHandle = new CompositeMotionHandle(Targets.Length);
			_savedLocalPos = Targets.Select(t => t.localPosition).ToArray();
			
			foreach (var target in Targets)
			{
				if (Shake)
				{
					Easing = Ease.Linear;
					LMotion
						.Shake
						.Create(target.position, Strength, Duration)
						.WithLoops(InfinityLoops ? -1 : 1, LoopType)
						.WithEase(Easing)
						.WithCancelOnError()
						.BindToPosition(target)
						.AddTo(_compositeMotionHandle);
				}
				else
				{
					LMotion
						.Create(target.position, target.position + new Vector3(Random.value * Strength.x, Random.value * Strength.y, Random.value * Strength.z), Duration)
						.WithDelay(Random.value * RandomDelay)
						.WithLoops(InfinityLoops ? -1 : 1, LoopType)
						.WithEase(Easing)
						.WithCancelOnError()
						.BindToPosition(target)
						.AddTo(_compositeMotionHandle);
				}
			}
		}

		[Button]
		public void DisableAnim()
		{
			_compositeMotionHandle?.Cancel();
			
			if (_savedLocalPos is null)
				return;
			
			for (var i = 0; i < _savedLocalPos.Length && i < Targets.Length; i++)
			{
				Targets[i].localPosition = _savedLocalPos[i];
			}
		}

		/*[Button]
		public void SmoothDisableAnim()
		{
			LMotion
				.Create(1f, 0f, 0.25f)
				.WithOnComplete(OnComplete)
				.Bind(OnSmoothValueDisable);
		}
		
		private void OnComplete()
		{
			_compositeMotionHandle?.Cancel();
		}

		private void OnSmoothValueDisable(float value)
		{
			foreach (var motion in _compositeMotionHandle)
			{
				motion.PlaybackSpeed = value;
			}
		}*/
	}
}