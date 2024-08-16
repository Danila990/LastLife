using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui.Sandbox.Pointer
{
	public class PointerIcon : MonoBehaviour
	{
		[SerializeField] private Image _image;
		[SerializeField] private float _duration = 0.25f;

		private bool _isShown;
		private MotionHandle _handle;

		public void OnCreate()
		{
			_image.enabled = false;
		}
		
		public void SetIconPosition(Vector3 position, Quaternion rotation)
		{
			transform.SetPositionAndRotation(position, rotation);
		}

		public void Show()
		{
			if (_isShown)
				return;
			_isShown = true;
			ShowProcess();
		}

		public void Hide()
		{
			if (!_isShown)
				return;
			_isShown = false;

			HideProcess();
		}

		private void ShowProcess()
		{
			_image.enabled = true;
			_handle.IsActiveComplete();
			_handle = LMotion
				.Create(Vector3.zero, Vector3.one, _duration)
				.BindToLocalScale(transform);
		}

		private void HideProcess()
		{
			_handle.IsActiveComplete();
			_handle = LMotion
				.Create(Vector3.one, Vector3.zero, _duration)
				.WithOnComplete(() => _image.enabled = false)
				.BindToLocalScale(transform);
		}

		private void OnDisable()
		{
			_handle.IsActiveCancel();
		}
	}
}
