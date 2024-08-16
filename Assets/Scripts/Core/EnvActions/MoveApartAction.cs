using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.EnvActions
{
	public class MoveApartAction : EnvironmentAction
	{
		[SerializeField][BoxGroup("Part 1")][HideLabel][LabelText("Offset")] private Vector3 _offset1;
		[SerializeField][BoxGroup("Part 1")][HideLabel][LabelText("Origin")] private Transform _origin1;
		[SerializeField][BoxGroup("Part 2")][HideLabel][LabelText("Offset")] private Vector3 _offset2;
		[SerializeField][BoxGroup("Part 2")][HideLabel][LabelText("Origin")] private Transform _origin2;
		[SerializeField] private float _duration;

		private MotionHandle _handle1;
		private MotionHandle _handle2;
		
		private void OnDisable()
		{
			_handle1.IsActiveCancel();
			_handle2.IsActiveCancel();
		}
		
		[Button]
		public override void Execute()
		{
			_handle1.IsActiveCancel();
			_handle2.IsActiveCancel();
			
			var position1 = _origin1.localPosition;
			var position2 = _origin2.localPosition;
			
			_handle1 = LMotion
				.Create(position1, position1 + _offset1, _duration)
				.BindToLocalPosition(_origin1);
			
			_handle2 = LMotion
				.Create(position2, position2 + _offset2, _duration)
				.BindToLocalPosition(_origin2);
		}
	}
}
