using System;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.LoadScreen
{
	public class LoadScreenView : UiView
	{
		public RectTransform Spinner;
		public TextMeshProUGUI Text;
		public Image ProgressImage;
	}

	public class LoadScreenController : UiController<LoadScreenView>, IProgress<float>
	{
		private MotionHandle _handle;
		private MotionHandle _rotationHandle;
		private MotionHandle _textMotion;


		public override void OnShow()
		{
			View.ProgressImage.fillAmount = 0;
			View.Spinner.localRotation = Quaternion.identity;
			
			_rotationHandle.IsActiveCancel();
			_rotationHandle = LMotion
				.Create(Vector3.zero, new Vector3(0f, 0f, -360), 1f)
				.WithLoops(-1, LoopType.Incremental)
				.BindToLocalEulerAngles(View.Spinner);

			_textMotion.IsActiveCancel();
			_textMotion =
				LMotion.String
					.Create32Bytes("Loading", "Loading...", 1f)
					.WithDelay(0.2f)
					.WithLoops(-1)
					.BindToText(View.Text);
		}

		public override void OnHide()
		{
			_handle.IsActiveCancel();
			_textMotion.IsActiveCancel();
			_rotationHandle.IsActiveCancel();
		}
		
		public void Report(float value)
		{
			if (Math.Abs(value - View.ProgressImage.fillAmount) < 0.01f)
				return;
			
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(View.ProgressImage.fillAmount, value, 0.15f)
				.BindToFillAmount(View.ProgressImage);
		}
	}
}