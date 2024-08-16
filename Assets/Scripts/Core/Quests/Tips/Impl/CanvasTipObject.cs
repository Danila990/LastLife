using System;
using Core.Quests.Tips.Impl.Interfaces;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Quests.Tips.Impl
{
	public class CanvasTipObject : QuestTipObject, ITrackedTip
	{
		[SerializeField] private RectTransform _rectTransform;
		
		[SerializeField] [BoxGroup("Anim")]
		private RectTransform _tip;
		[SerializeField] [BoxGroup("Anim")]
		private float _duration;
		[SerializeField] [BoxGroup("Anim")]
		private Vector3 _offset;

		private MotionHandle _handle;
		private Vector3 _initialPosition;

		protected override void OnCreatedInternal()
		{
			_initialPosition = _tip.anchoredPosition;
		}

		public void SetTrackedTarget(Transform target, Vector3 offset, Vector3 eulerAngels)
		{
			_rectTransform.SetParent(target);
			Vector2 offsetV2 = offset; 
			_rectTransform.anchoredPosition = offsetV2;
			_rectTransform.localEulerAngles = eulerAngels;
		}

		private void OnDisable()
		{
			_handle.IsActiveCancel();
		}

		public override void OnRent()
		{
			_tip.anchoredPosition = _initialPosition;
			base.OnRent();
			PlayTween();
		}

		public override void OnReturn()
		{
			_handle.IsActiveCancel();
			base.OnReturn();
		}

		private void PlayTween()
		{
			_handle.IsActiveCancel();
			_handle = LMotion.Create(_initialPosition, _initialPosition + _offset, _duration)
				.WithLoops(-1, LoopType.Yoyo)
				.BindToAnchoredPosition3D(_tip);
		}
	}
}
