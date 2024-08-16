using System;
using UnityEngine;

namespace Core.Inventory
{
	public class SimpleTimerAction
	{
		private float _timePassed;
		private Action _action;
		private bool _canUse;
		
		public float Time { get; private set; }

		public SimpleTimerAction(float time)
		{
			Time = time;
		}

		public void SetTime(float time)
		{
			Time = time;
		}
		
		public SimpleTimerAction(float time, float timePassed)
		{
			Time = time;
			_timePassed = time;
		}
		

		public void SetAction(Action action) => _action = action;

		public void Tick(ref float deltaTime)
		{
			_timePassed += deltaTime;
			if (_timePassed < Time)
				return;
			UseAction();
		}

		public void CanUse(bool status)
		{
			_canUse = status;
		}

		private void UseAction()
		{
			if (!_canUse)
				return;
			Reset();
			_action?.Invoke();
		}

		public void Reset() => _timePassed = 0.0f;
	}
}