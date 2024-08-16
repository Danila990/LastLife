using System;
using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Entity.Ai.Movement
{
	public class FlyingAiMovementController : AiMovementController
	{
		[SerializeField] private FlyingAgent _agent;
		[SerializeField] private float _stoppingMlp;
		[SerializeField] private float _blockTimeToResetPath;

		[SerializeField, BoxGroup("Wander")] private float _minWanderDistance;
		[SerializeField, BoxGroup("Wander")] private float _maxWanderDistance;
		
		private IPathfindingService _pathfindingService;

		public bool IsRotateByPath = true;
		
		[Title("Debug")]
		[ShowInInspector] private bool _isEnabled = true;
		[ShowInInspector] private readonly List<Vector3> _path = new List<Vector3>();
		[ShowInInspector] private int _pathIndex;
		
		private bool _blockPathFinding;
		private float _lastTimePathIndexChanged;
		private IAiTarget _aiTarget;

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			for (var index = 0; index < _path.Count; index++)
			{
				var point = _path[index];
				if (index + 1 >= _path.Count)
					break;
				
				Gizmos.DrawLine(point, _path[index + 1]);
			}
		}
#endif

		private void FixedUpdate()
		{
			if (!_isEnabled)
				return;
			
			MoveByPath();
			if (!IsRotateByPath)
			{
				var valueLookAtPoint = _aiTarget.LookAtPoint;
				var delta = (valueLookAtPoint - transform.position).normalized;
				delta.y = 0;
				RotateToDir(delta);
			}
		}

		public override void SetDestination(Vector3 point)
		{
			if (_blockPathFinding)
				return;
			_pathfindingService.GetPath(_path, transform.position, point);
			
			if (_path.Count == 0)
			{
				AgentStatusType = AgentStatusType.Waiting;
			}
			else
			{
				AgentStatusType = AgentStatusType.MoveByPath;
				_lastTimePathIndexChanged = Time.time;
			}
			ResetPathfindingTimer().Forget();
		}

		public void SetPathManually(IReadOnlyList<Vector3> path)
		{
			_path.Clear();
			_path.AddRange(path);
			AgentStatusType = AgentStatusType.MoveByPath;
			_lastTimePathIndexChanged = Time.time;
			ResetPathfindingTimer().Forget();
		}

		private async UniTaskVoid ResetPathfindingTimer()
		{
			_blockPathFinding = true;
			await UniTask.Delay(TimeSpan.FromSeconds(1));
			_blockPathFinding = false;
		}
		
		public override void Created(IObjectResolver resolver)
		{
			_pathfindingService = resolver.Resolve<IPathfindingService>();
		}
		
		public override void Disable()
		{
			_isEnabled = false;
			ResetPathfinding();
			_agent.IsFlying = false;
		}
		
		public override void ResetPath()
		{
			ResetPathfinding();
		}

		public override void Wander()
		{
			var min = _minWanderDistance;
			var max = _maxWanderDistance;
			min = Mathf.Clamp(min, 0.01f, max);
			max = Mathf.Clamp(max, min, max);
			var wanderPos = transform.position;
			
			while ( ( wanderPos - transform.position ).magnitude < min )
			{
				wanderPos = (Random.insideUnitSphere * max) + transform.position;
			}
			SetDestination(wanderPos);
		}

		private void MoveByPath()
		{
			if (_path.Count == 0)
				return;
			
			if (_pathIndex >= _path.Count - 1)
			{
				ResetPathfinding();
				return;
			}
			
			var dirToPoint = _path[_pathIndex] - transform.position;
			if (dirToPoint.sqrMagnitude < StoppingDistance)
			{
				_pathIndex++;
				_lastTimePathIndexChanged = Time.time;
				return;
			}
			
			var vel = dirToPoint.normalized;
			var dir = vel;
			dir.y = 0;
			
			_agent.CurrSpeed += (Speed - _agent.CurrSpeed) * 0.3f * Time.fixedDeltaTime;
			_agent.Velocity = vel * _agent.CurrSpeed;

			if (Time.time - _lastTimePathIndexChanged > _blockTimeToResetPath)
			{
				ResetPathfinding();
				return;
			}
			if (IsRotateByPath)
			{
				RotateToDir(dir);
			}
		}
		
		public void RotateToDir(Vector3 dir)
		{
			Util.ForDebug(transform.position, dir);
		
			var target = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.fixedDeltaTime * 5);
		}
		
		protected void ResetPathfinding()
		{
			AgentStatusType = AgentStatusType.Waiting;
			_agent.CurrSpeed *= _stoppingMlp;
			_path.Clear();
			_pathIndex = 1;
		}
		
		public void SetRotationTarget(IAiTarget aiTarget)
		{
			_aiTarget = aiTarget;
		}
	}
}