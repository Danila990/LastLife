using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Ai.Movement
{
	public class FlyingAgent : MonoBehaviour
	{
		[SerializeField] private Rigidbody _rigidbody;
		[field:ShowInInspector] public float CurrSpeed { get; set; }
		[field:SerializeField] public bool IsFlying { get; set; }
		[field: SerializeField] public float MaxFlyHeight { get; set; } = 10f;

#pragma warning disable CS0657
		[field:ShowInInspector, HideInEditorMode]
		public Vector3 Velocity 
		{ 
			get => _rigidbody.velocity;
			set => _rigidbody.velocity = value;
		}

		private void FixedUpdate()
		{
			Fly();
		}

		private void Fly()
		{
			if (!IsFlying)
				return;

			_rigidbody.velocity *= 0.98f;
			if (transform.position.y < MaxFlyHeight)
			{
				_rigidbody.velocity += -Physics.gravity * Time.deltaTime;
			}
			_rigidbody.angularVelocity = Vector3.zero;
			CurrSpeed -= Time.deltaTime * Time.deltaTime;
		}
	}
}