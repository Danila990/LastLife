using UnityEngine;

namespace Core.Entity.Ai.Sensor.ColliderSensor
{
	public abstract class ColliderDetectionObserver : MonoBehaviour, IColliderDetectionHandler
	{
		[SerializeField] protected ColliderDetection _colliderDetection;

		private void OnEnable()
		{
			_colliderDetection.AddListener(this);
		}

		private void OnDisable()
		{
			_colliderDetection.RemoveListener(this);
		}

		public abstract void OnBufferUpdate(Collider[] colliders, in int size);
	}

}