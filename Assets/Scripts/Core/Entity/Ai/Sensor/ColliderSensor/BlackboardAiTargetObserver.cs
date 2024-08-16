using Core.Entity.Ai.TargetFinder;
using Core.Entity.Characters.Adapters;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core.Entity.Ai.Sensor.ColliderSensor
{
	
	public class BlackboardAiTargetObserver : ColliderDetectionObserver
	{
		[SerializeField] private string _parameterName;
		[SerializeField] private Blackboard _blackboard;
		[SerializeField] private AiTargetSelector _aiTargetSelector;
		public Faction TargetFaction;
		private Variable<IAiTarget> _variable;

		private void Start()
		{
			_variable = _blackboard.GetVariable<IAiTarget>(_parameterName);
			_colliderDetection.Scanning().Forget();
		}

		public override void OnBufferUpdate(Collider[] colliders, in int size)
		{
			if (size <= 0 || _variable.value != null)
				return;
			
			// Util.ColliderByDistanceSort.SetPos(transform.position);
			// Array.Sort(colliders, 0, size, Util.ColliderByDistanceSort);
			
			for (var i = 0; i < size; i++)
			{
				var col = colliders[i];
				col.transform.TryGetComponent(out IEntityAdapter entityAdapter);
				if (entityAdapter != null && entityAdapter.Entity)
				{
					SetVar(entityAdapter);
				}
				else if (col.attachedRigidbody && col.attachedRigidbody.TryGetComponent(out entityAdapter) && entityAdapter.Entity)
				{
					SetVar( entityAdapter);
				}
			}
		}
		
		private void SetVar(IEntityAdapter entityAdapter)
		{
			if ((entityAdapter.Entity.Faction & TargetFaction) != 0 &&
			    entityAdapter.Entity.TryGetAiTarget(out var aiTarget))
			{
				_aiTargetSelector.AddTarget(aiTarget);
			}
		}
	}
}