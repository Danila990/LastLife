using Core.Quests.Tips.Impl.Interfaces;
using UnityEngine;

namespace Core.Quests.Tips.Impl
{
	public class FxTipObject : QuestTipObject, ITrackedTip
	{
		[SerializeField] private ParticleSystem _fx;

		private Vector3 _offset;
		private Transform _target;
		
		private void LateUpdate()
		{
			if(!_target)
				return;

			transform.position = _target.position + _offset;
		}
		
		public void SetTrackedTarget(Transform target, Vector3 offset, Vector3 eulerAngels)
		{
			_target = target;
			_offset = offset;
		}
		
		public override void OnRent()
		{
			base.OnRent();
			_fx.Play();

		}

		public override void OnReturn()
		{
			if(!_fx.Equals(null))
				_fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			base.OnReturn();
			_target = null;
		}
	}

}
