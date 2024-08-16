using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.EnvActions
{
	public class RotateAction : EnvironmentAction
	{
		[SerializeField] private Vector3 _toRotate;
		[SerializeField] private Transform _root;
		[SerializeField] private float _duration;

		private MotionHandle _handle;
		
		private void OnDisable()
		{
			_handle.IsActiveCancel();
		}
		
		[Button]
		public override void Execute()
		{
			_handle = LMotion
					.Create(_root.transform.localRotation, Quaternion.Euler(_toRotate), _duration)
					.BindToLocalRotation(_root);
		}
	}

}
