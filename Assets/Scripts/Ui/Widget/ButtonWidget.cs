using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui.Widget
{
	public class ButtonWidget : MonoBehaviour
	{
		public Button Button;
		public Image BGImg;
		public Image IconImg;
		
		private MotionHandle _bgHandle;
		private MotionHandle _iconHandle;

		private void OnDisable()
		{
			_bgHandle.IsActiveCancel();
			_iconHandle.IsActiveCancel();
			OnDisableInternal();
		}

		protected virtual void OnDisableInternal()
		{
			
		}

		public void SelectAlpha(float duration = 0.2f)
		{
			/*if (!gameObject.activeInHierarchy)
				return;*/

			_bgHandle.IsActiveCancel();
			_bgHandle = LMotion
				.Create(BGImg.color.a, 1, duration)
				.BindToColorA(BGImg);
		}

		public void DeselectAlpha(float duration = 0.2f, float alpha = 0)
		{
			/*if (!gameObject.activeInHierarchy)
				return;*/
			
			_bgHandle.IsActiveCancel();
			_bgHandle = LMotion
				.Create(BGImg.color.a, alpha, duration)
				.BindToColorA(BGImg);
		}
		
		public void SelectAlphaForIcon(float duration = 0.2f)
		{
			_iconHandle.IsActiveCancel();
			
			if (duration <= 0)
			{
				var color = IconImg.color;
				color.a = 1;
				IconImg.color = color;
				return;
			}
			
			if (!gameObject.activeInHierarchy)
				return;
			
			_iconHandle = LMotion
				.Create(IconImg.color.a, 1, duration)
				.BindToColorA(IconImg);
		}

		public void DeselectAlphaForIcon(float duration = 0.2f, float alpha = 0)
		{
			_iconHandle.IsActiveCancel();
			if (duration <= 0)
			{
				var color = IconImg.color;
				color.a = alpha;
				IconImg.color = color;
				return;
			}
			if (!gameObject.activeInHierarchy)
				return;
			
			_iconHandle = LMotion
				.Create(IconImg.color.a, alpha, duration)
				.BindToColorA(IconImg);
		}
	}

}