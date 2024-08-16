using System;
using Core.Entity.Characters.Adapters;
using LitMotion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Entity.Ai.Ragdoll
{
	public class NavMeshRagdoll : IRagdollManager, IDisposable
	{
		private readonly NavMeshAgent _agent;
		private readonly Rigidbody _rigidbody;
		private readonly BaseHeadAdapter _baseHeadAdapter;
		private readonly IDisposable _collisionDisposable;
		private RagdollState _ragdollState;
		private bool _awaitingStandUp;
		private bool _isDead;
		private MotionHandle _handle;

		public NavMeshRagdoll(NavMeshAgent agent, Rigidbody rigidbody, BaseHeadAdapter baseHeadAdapter)
		{
			_agent = agent;
			_rigidbody = rigidbody;
			_baseHeadAdapter = baseHeadAdapter;
			_collisionDisposable = rigidbody
				.OnCollisionEnterAsObservable()
				.Subscribe(OnCollision);
		}
		
		private void OnCollision(Collision obj)
		{
			if (_ragdollState == RagdollState.Ragdoll && !_handle.IsActive())
			{
				_handle = LMotion.Create(0, 1, 3f).WithOnComplete(Callback).RunWithoutBinding();
			}
		}
		
		private void Callback()
		{
			SetState(RagdollState.Normal);
		}

		public void SetState(RagdollState ragdollState)
		{
			if (_isDead)
				return;
			
			switch (ragdollState)
			{
				case RagdollState.Normal:
					DisableRagDoll();
					break;
				case RagdollState.Drag:
					if (_handle.IsActive())
						_handle.Cancel();
					EnableRagDoll();
					break;
				case RagdollState.Ragdoll:
					EnableRagDoll();
					break;
			}
			_ragdollState = ragdollState;
		}
		
		public void EnableRagDoll()
		{
			_baseHeadAdapter.DisableControl();
			_agent.enabled = false;
			_rigidbody.isKinematic = false;
		}
		
		public void DisableRagDoll()
		{
			if (_handle.IsActive())
				_handle.Cancel();
			
			_baseHeadAdapter.EnableControl();
			_agent.enabled = true;
			_rigidbody.isKinematic = true;
			_agent.transform.up = Vector3.up;
		}
		
		public void Death()
		{
			_collisionDisposable?.Dispose();
			//DisableRagDoll();
			_isDead = true;
		}
		
		public void Dispose()
		{
			_collisionDisposable?.Dispose();
			if (_handle.IsActive())
			{
				_handle.Cancel();
			}
		}
	}
}