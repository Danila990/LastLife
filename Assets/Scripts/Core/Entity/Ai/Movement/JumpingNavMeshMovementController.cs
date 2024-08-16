using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Entity.Ai.Movement
{
	public class JumpingNavMeshMovementController : AiMovementController
	{
		[ShowInInspector, PropertyOrder(-1)] public JumpingState JumpingStatus { get; private set; }
		
		[SerializeField, BoxGroup("Refs")] private Rigidbody _rigidbody;
		[SerializeField, BoxGroup("Refs")] private NavMeshAgent _navMeshAgent;

		[SerializeField, BoxGroup("Parameters")] private float _upJumpForce;
		[SerializeField, BoxGroup("Parameters")] private float _forwardJumpForce;
		[SerializeField, BoxGroup("Parameters")] private float _additionalJumpForce;
		[SerializeField, BoxGroup("Parameters")] private float _jumpInterval;
		[SerializeField, BoxGroup("Parameters")] private float _nextPathTimeInterval;
		
		[BoxGroup("Wander")] public float _minWanderDistance = 5;
		[BoxGroup("Wander")] public float _maxWanderDistance = 20;
		
		private NavMeshPath _path;
		
		[FoldoutGroup("Debug")] [ShowInInspector] private int _cornerIndex;
		[FoldoutGroup("Debug")] [ShowInInspector] private Vector3[] _corners;
		[FoldoutGroup("Debug")] [ShowInInspector] private bool _shouldMove;
		[FoldoutGroup("Debug")] [ShowInInspector] private Vector3 _lastTargetPosition;
		[FoldoutGroup("Debug")] [ShowInInspector] private float _nextPathTime;
		[FoldoutGroup("Debug")] [ShowInInspector] private Vector3 _nextPoint;
		[FoldoutGroup("Debug")] [ShowInInspector] private float _nextJumpTime;

		private void Awake()
		{
			_path = new NavMeshPath();
			_cornerIndex = 0;
			_corners = Array.Empty<Vector3>();
			_lastTargetPosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
			_navMeshAgent.updatePosition = false;
			_navMeshAgent.updateRotation = false;
			_navMeshAgent.isStopped = true;
		}

		private void FixedUpdate()
		{
			if (!_shouldMove)
				return;

			UpdateState();
			if (_nextPoint == Vector3.zero)
				return;
			var dir = (_nextPoint - transform.position).normalized;
			Jump(dir);
			dir.y = 0;
			//_rigidbody.rotation = Quaternion.Lerp(_rigidbody.rotation, Quaternion.LookRotation(dir), Time.fixedDeltaTime * 5);
		}
		
		public override void Created(IObjectResolver resolver)
		{
			
		}
		
		public override void Disable()
		{
			Stop();
		}
		
		public override void ResetPath()
		{
			Stop();
		}
		
		public override void Wander()
		{
			var min = _minWanderDistance;
			var max = _maxWanderDistance;
			min = Mathf.Clamp(min, 0.01f, max);
			max = Mathf.Clamp(max, min, max);
			var wanderPos = transform.position;
			while ( ( wanderPos - transform.position ).magnitude < min ) {
				wanderPos = ( Random.insideUnitSphere * max ) + transform.position;
			}

			if ( NavMesh.SamplePosition(wanderPos, out var hit, _navMeshAgent.height * 2, NavMesh.AllAreas) )
			{
				SetDestination(hit.position);
				AgentStatusType = AgentStatusType.MoveByPath;
			}
		}
		
		private void Jump(Vector3 dir)
		{
			if (Time.time < _nextJumpTime)
				return;
			
			if (_navMeshAgent.enabled)
			{
				_navMeshAgent.enabled = false;
			}
			
			var vector = Quaternion.LookRotation(dir.normalized) * new Vector3(0, (_upJumpForce + _additionalJumpForce),  (_forwardJumpForce + _additionalJumpForce / 2f)) ;
			_additionalJumpForce = 0;
			_rigidbody.AddForce(vector, ForceMode.Acceleration);
			_nextJumpTime = Time.time + _jumpInterval;
		}
		
		public override void SetDestination(Vector3 point)
		{
			_lastTargetPosition = point;
			// var vector = Vector3.up * (_upJumpForce + _additionalJumpForce) + transform.right * (_forwardJumpForce + _additionalJumpForce / 2f);
			// _additionalJumpForce = 0;
			// _rigidbody.AddForce(vector, ForceMode.Acceleration);
			SetMovingStatus(true);
		}

		public void SetMovingStatus(bool status)
		{
			JumpingStatus = JumpingState.Seeking;
			_shouldMove = status;
			if (!_shouldMove)
			{
				Stop();
			}
		}

		private void UpdateState()
		{
			if (!_navMeshAgent.enabled)
			{
				_navMeshAgent.enabled = false;
			}
			switch (JumpingStatus)
			{
				case JumpingState.Seeking:
					//normalizedDeltaPosition = Vector3.zero;
					if (Time.time > _nextPathTime) 
						CalculatePath(_lastTargetPosition);
					
					if (_path.status == NavMeshPathStatus.PathComplete)
					{
						_corners = _path.corners;
						_cornerIndex = 0;

						if (_corners.Length == 0)
						{
							Debug.LogWarning("Zero Corner Path", transform);
						}
						else
						{
							JumpingStatus = JumpingState.OnPath;
						}
					}

					if (_path.status == NavMeshPathStatus.PathPartial)
					{
						Debug.LogWarning("Path Partial", transform);
					}

					if (_path.status == NavMeshPathStatus.PathInvalid)
					{
						Debug.LogWarning("Path Invalid", transform);
					}
					break;
				
				case JumpingState.OnPath:
					if (Time.time > _nextPathTime)
					{
						CalculatePath(_lastTargetPosition);
						break;
					}

					if (_cornerIndex < _corners.Length)
					{
						_nextPoint = _corners[_cornerIndex];
						Vector3 d = _nextPoint - transform.position;
						float mag = d.magnitude;

						if (mag < _navMeshAgent.stoppingDistance)
						{
							_cornerIndex++;

							if (_cornerIndex >= _corners.Length) 
								CalculatePath(_lastTargetPosition);
						}
					}
					break;
			}
		}
		
		private void CalculatePath(Vector3 targetPosition)
		{
			if (!_navMeshAgent.enabled)
			{
				_navMeshAgent.enabled = true;
			}
			if (!_navMeshAgent.CalculatePath(targetPosition, _path))
			{
				NavMesh.SamplePosition(targetPosition, out var hit, 10, NavMesh.AllAreas);
				_navMeshAgent.CalculatePath(hit.position, _path);
			}
			_navMeshAgent.enabled = false;
			JumpingStatus = JumpingState.Seeking;
			_nextPathTime = Time.time + _nextPathTimeInterval;
		}
		
		private void Stop()
		{
			_shouldMove = false;
			JumpingStatus = JumpingState.Idle;
			_nextPoint = Vector3.zero;
			_navMeshAgent.enabled = true;
		}
		
		public enum JumpingState
		{
			Idle,
			Seeking,
			OnPath,
		}


		private void OnDrawGizmosSelected()
		{
			if (_corners is null || _corners.Length == 0)
				return;	
			if (JumpingStatus == JumpingState.Idle) Gizmos.color = Color.gray;
			if (JumpingStatus == JumpingState.Seeking) Gizmos.color = Color.red;
			if (JumpingStatus == JumpingState.OnPath) Gizmos.color = Color.green;

			if (_corners.Length > 0 && JumpingStatus == JumpingState.OnPath && _cornerIndex == 0)
			{
				Gizmos.DrawLine(transform.position, _corners[0]);
			}

			for (int i = 0; i < _corners.Length; i++)
			{
				Gizmos.DrawSphere(_corners[i], 0.1f);
			}

			if (_corners.Length > 1)
			{
				for (int i = 0; i < _corners.Length - 1; i++)
				{
					Gizmos.DrawLine(_corners[i], _corners[i + 1]);
				}
			}

			Gizmos.color = Color.white;
		}
	}
}