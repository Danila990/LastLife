using System.Collections.Generic;
using BurstLinq;
using NodeCanvas.Framework;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.TargetFinder
{
	public class ByDistanceTargetAiSelector : AiTargetSelector
	{
		[SerializeField] private string _parameterName;
		[SerializeField] private Blackboard _blackboard;
		private readonly Stack<uint> _toDestroy = new Stack<uint>();
		private readonly List<uint> _ignoredTargets = new List<uint>();
		
		private Variable<IAiTarget> _variable;
		private float _lastRecalcTime;
		private float _clearIgnoredNextTime;

		private void Start()
		{
			_variable = _blackboard.GetVariable<IAiTarget>(_parameterName);
			Observable
				.Interval(0.5f.ToSec())
				.Subscribe(OnUpdate)
				.AddTo(this);
		}

		public override void AddTarget(IAiTarget target)
		{
			if (BurstLinqExtensions.Contains(_ignoredTargets, target.Uid))
				return;
			base.AddTarget(target);
		}

		private void OnUpdate(long obj)
		{
			RecalculateTarget();
		}

		public override void RecalculateTarget()
		{
			if (Time.time > _clearIgnoredNextTime)
			{
				_clearIgnoredNextTime = Mathf.Infinity;
				_ignoredTargets.Clear();
			}
			
			var maxPriority = float.NegativeInfinity;
			AiTargetControl resultTarget = null;
			var pos = transform.position;
			
			var timeDelta = Time.time - _lastRecalcTime;
			_lastRecalcTime = Time.time;

			foreach (var target in Targets.Values) 
			{
				if (!target.AiTarget.IsActive || target.CurrentPriority < -5f)
				{
					_toDestroy.Push(target.AiTarget.Uid);
					continue;
				}
				var priority = target.GetPriority(pos);
				target.UpdateTargetPriority(timeDelta);
				
				if (priority > maxPriority)
				{
					resultTarget = target;
					maxPriority = priority;
				}
			}
			
			while (_toDestroy.TryPop(out var uid))
			{
				_ignoredTargets.Add(uid);
				RemoveTarget(uid);
				_clearIgnoredNextTime = _lastRecalcTime + 3f;
			}

			if (resultTarget != null)
			{
				_variable.value = resultTarget.AiTarget;
			}
		}
	}
}