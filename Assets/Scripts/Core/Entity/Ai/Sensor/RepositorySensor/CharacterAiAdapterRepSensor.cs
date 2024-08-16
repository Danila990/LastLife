using System.Collections.Generic;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Ai.Sensor.RepositorySensor
{
	public class CharacterAiAdapterRepSensor : AbstractRepositorySensor<LifeEntity>
	{
		[SerializeField] private string _parameterName;
		[SerializeField] private Blackboard _blackboard;
		[SerializeField] private Faction _targetFaction = Faction.Heads;

		[TitleGroup("Vision Settings")]
		public float maxDistance = 50;
		[Tooltip("A layer mask to use for line of sight check.")]
		public LayerMask envMask = (LayerMask)( -1 );
		[Tooltip("Distance within which the target can be seen (or rather sensed) regardless of view angle.")]
		public float awarnessDistance = 0f;
		[Range(1, 180)]
		public float viewAngle = 70f;
		[SerializeField] private Vector3 offset;
		private Variable<IAiTarget> _variable;

		protected override void OnInit()
		{
			_variable = _blackboard.GetVariable<IAiTarget>(_parameterName);
			Scanning().Forget();
		}

		public override void OnBufferUpdate(ICollection<LifeEntity> observedCollection)
		{
			foreach (var headContext in observedCollection)
			{
				if (ValidateContext(headContext))
				{
					SetTarget(headContext);
				}
			}
		}

		private void SetTarget(EntityContext headContext)
		{
			if (!headContext.TryGetAiTarget(out var target))
				return;
			
			if (_variable.value is { IsActive: true })
				return;
			_variable.value = target;
		}
		
		private bool ValidateContext(LifeEntity headContext)
		{
			if ( headContext.Health.IsDeath || headContext.Faction != _targetFaction) {
				return false;
			}
			
			var movePoint = headContext.LookAtTransform.position;
			

			if ( Vector3.Distance(transform.position, movePoint) <= awarnessDistance)
			{
				return !Physics.Linecast(transform.position + offset, movePoint + offset, envMask.value);
			}

			if ( Vector3.Distance(transform.position, movePoint) > maxDistance ) {
				return false;
			}

			if ( Vector3.Angle(movePoint - transform.position, transform.forward) > viewAngle ) {
				return false;
			}

			return !Physics.Linecast(transform.position + offset, movePoint + offset, envMask.value);

		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawLine(transform.position, transform.position + offset);
			Gizmos.DrawLine(transform.position + offset, transform.position + offset + ( transform.forward * maxDistance ));
			Gizmos.DrawWireSphere(transform.position + offset + ( transform.forward * maxDistance ), 0.1f);
			Gizmos.DrawWireSphere(transform.position, awarnessDistance);
			Gizmos.matrix = Matrix4x4.TRS(transform.position + offset, transform.rotation, Vector3.one);
			Gizmos.DrawFrustum(Vector3.zero, viewAngle, maxDistance, 0, 1f);
		}
	}
}