using System;
using System.Collections;
using UnityEngine;

namespace Tests.UITest.Conditions
{
	public class WaitCondition : IEnumerable
	{
		private readonly ICondition _condition;
		private readonly float _timeout;
		private float _time;

		public WaitCondition(ICondition condition, float timeout)
		{
			_condition = condition;
			_timeout = timeout;
		}

		public IEnumerator GetEnumerator()
		{
			while (!_condition.Satisfied())
			{
				if (_time > _timeout)
					throw new Exception($"Operation timed out ({_timeout}s): {_condition}");
				_time += Time.unscaledDeltaTime;
				yield return null;
			}
		}
	}
}