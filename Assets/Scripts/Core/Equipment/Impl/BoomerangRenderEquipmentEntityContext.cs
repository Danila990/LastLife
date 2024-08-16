using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Impl
{
	public class BoomerangRenderEquipmentEntityContext : RenderEquipmentEntityContext
	{
		public Transform _model;
		[SerializeField, BoxGroup("Params")] private float _amplitude;
		[SerializeField, BoxGroup("Params")] private float _inactiveRotationSpeed;

		private void Update()
		{
			if(!IsEquipped)
				return;
			
			_model.localPosition += new Vector3(0, Mathf.Sin(Time.time * _amplitude) * .2f * Time.deltaTime, 0);
			_model.Rotate(0, 0, Time.deltaTime * _inactiveRotationSpeed);
		}
	}
}