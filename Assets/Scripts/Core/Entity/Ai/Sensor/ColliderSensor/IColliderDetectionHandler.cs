using UnityEngine;

namespace Core.Entity.Ai.Sensor.ColliderSensor
{
	public interface IColliderDetectionHandler
	{
		void OnBufferUpdate(Collider[] colliders, in int size);
	}
}