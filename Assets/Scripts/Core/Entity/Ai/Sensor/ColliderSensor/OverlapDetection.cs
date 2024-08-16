using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Ai.Sensor.ColliderSensor
{
	public class OverlapDetection : ColliderDetection
	{
		[SerializeField] private Transform _detectionPoint;
		[SerializeField] private float _radius;
		
		[ValueDropdown("@LayerMasks.GetLayersMasks()")]
		[SerializeField] private int _layer;
		[SerializeField]
		private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.UseGlobal;
		
		public float Radius => _radius;
		
		protected override int Detect(Collider[] buffer)
		{
			return Physics.OverlapSphereNonAlloc(
				_detectionPoint.position,
				_radius,
				buffer,
				_layer,
				_triggerInteraction
				);
		}
		
#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			var color = IsScanning ? Color.red : Color.blue;
			Gizmos.color = color;
			var point = Vector3.zero;
			if (_detectionPoint)
			{
				point = _detectionPoint.position;
			}
			Gizmos.DrawWireSphere(point, _radius);
		}
#endif
	}
}