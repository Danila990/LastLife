using UnityEngine;
namespace VFX
{
	public class EmitParticlesOnAxis : MonoBehaviour
	{
		public ParticleSystem ParticleSystem;
		public float DeltaThreshold = 1.0f;
		private Vector3 _lastPosition;
		private float _distance;

		private void Start()
		{
			if (ParticleSystem == null)
			{
				ParticleSystem = GetComponent<ParticleSystem>();
			}
			_lastPosition = transform.position;
		}

		private void Update()
		{
			_distance = Mathf.Abs(transform.position.y - _lastPosition.y);

			if (_distance >= DeltaThreshold)
			{
				ParticleSystem.Emit(2);
			}

			_lastPosition = transform.position;
		}
	}
}
