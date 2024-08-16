using System;
using LitMotion;
using UnityEngine;

namespace Core.AnimationRigging
{
	public class RigElementController : IDisposable
	{
		public readonly RigData RigData;
		public readonly Transform RigTarget;
		
		private event Action<float> OnSetWeight;
		private event Action OnDisable;
		private event Action OnEnable;
		
		private MotionHandle _enableTweener;
		private MotionHandle _disableTweener;
		public event Action<bool> OnComplete;
		
		public RigElementController(RigData rigData, Transform rigTarget)
		{
			RigData = rigData;
			RigTarget = rigTarget;
			
			OnSetWeight = SetWeight;
			OnDisable = () => OnComplete?.Invoke(false);
			OnEnable = () => OnComplete?.Invoke(true);
		}

		public ref MotionHandle EnableRig()
		{
			_enableTweener = TweenInternal(ref _enableTweener,ref _disableTweener, 1f, RigData.EnableTime, RigData.EnableDelay, OnEnable);
			return ref _enableTweener;
		}
		
		private ref MotionHandle TweenInternal(
			ref MotionHandle currentTweener,
			ref MotionHandle other,
			float endValue,
			float time,
			float delay,
			Action onComplete)
		{
			if (other.IsActive())
			{
				other.Cancel();
			}
			
			if (currentTweener.IsActive())
			{
				currentTweener.Cancel();
			}
			
			currentTweener = LMotion
				.Create(RigData.Rig.weight, endValue, time)
				.WithDelay(delay)
				.WithScheduler(MotionScheduler.PostLateUpdate)
				.WithOnComplete(onComplete)
				.Bind(OnSetWeight);
			
			
			return ref currentTweener;
		}
		

		public void TryDisable()
		{
			if (_disableTweener.IsActive())
				return;

			DisableRig();
		}
		
		public void TryEnable()
		{
			if (_enableTweener.IsActive())
				return;

			EnableRig();
		}
		
		public ref MotionHandle DisableRig()
		{
			_disableTweener = TweenInternal(ref _disableTweener,ref _enableTweener, 0f, RigData.DisableTime, RigData.DisableDelay, OnDisable);
			return ref _disableTweener;
		}
		
		private void SetWeight(float value)
		{
			RigData.Rig.weight = value;
		}
		
		public void Dispose()
		{
			if (_enableTweener.IsActive())
			{
				_enableTweener.Cancel();
			}
			
			if (_disableTweener.IsActive())
			{
				_disableTweener.Cancel();
			}
			
			OnSetWeight = null;
			OnDisable = null;
			OnEnable = null;
			OnComplete = null;
		}
	}
}