using UnityEngine;

namespace Common
{
	public class PhysicConveyor : MonoBehaviour 
	{
		private Rigidbody _rigidbody;
		public float speed = 2.0f;

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}
		
		private void FixedUpdate()
		{
			_rigidbody.position -= transform.forward * (speed * Time.fixedDeltaTime);
			_rigidbody.MovePosition(_rigidbody.position + transform.forward * (speed * Time.fixedDeltaTime));
		}
	}
}