using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Entity.Ai.TargetFinder
{
	public abstract class AiTargetSelector : MonoBehaviour
	{
		private readonly Dictionary<uint, AiTargetControl> _targets = new Dictionary<uint, AiTargetControl>();

		public IReadOnlyDictionary<uint, AiTargetControl> Targets => _targets;
		public abstract void RecalculateTarget();
		
		public virtual void AddTarget(IAiTarget target)
		{
			if (_targets.ContainsKey(target.Uid))
				return;
			_targets.Add(target.Uid, AiTargetControl.GetInstance(target, 10));
		}
		
		public void AddTargetPriority(IAiTarget target, float priority)
		{
			AddTarget(target);
			if (_targets.TryGetValue(target.Uid, out var t))
			{
				t.AddPriority(priority);
			}
		}

		public void RemoveTarget(uint uid)
		{
			_targets.Remove<uint, AiTargetControl>(uid, out var value);
			AiTargetControl.Release(value);
		} 

		public class AiTargetControl
		{
			public IAiTarget AiTarget;
			public float CurrentPriority;
			
			public float GetPriority(Vector3 pos)
			{
				return -Vector3.Distance(pos, AiTarget.MovePoint) + CurrentPriority;
			}
			
			public void UpdateTargetPriority(float deltaTime)
			{
				CurrentPriority -= deltaTime;
			}
			
			public void AddPriority(float priority)
			{
				CurrentPriority += priority;
			}

			public static AiTargetControl GetInstance(IAiTarget aiTarget, float currentPriority)
			{
				var aiTargetControl = GenericPool<AiTargetControl>.Get();
				aiTargetControl.AiTarget = aiTarget;
				aiTargetControl.CurrentPriority = currentPriority;
				return aiTargetControl;
			}
			
			public static void Release(AiTargetControl aiTargetControl)
			{
				aiTargetControl.AiTarget = null;
				aiTargetControl.CurrentPriority = default;
				GenericPool<AiTargetControl>.Release(aiTargetControl);
			}
		}
	}
}