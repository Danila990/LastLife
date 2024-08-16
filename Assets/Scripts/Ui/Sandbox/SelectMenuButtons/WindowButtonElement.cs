using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Sandbox.SelectMenuButtons
{
	public class WindowButtonElement : MonoBehaviour
	{
		[SerializeField] private Button _button;
		public Image Outline;
		public string MenuName;
		public event Action<WindowButtonElement> OnClick;
		
		private void OnEnable()
		{
			if (Outline)
			{
				Outline.enabled = false;
			}
		}
		
		public void SelectOutline()
		{
			Outline.enabled = true;
		}

		public void DeselectOutline()
		{
			Outline.enabled = false;
		}
		
		public void Init()
		{
			_button.onClick.AddListener(OnClickButton);
		}
		
		private void OnClickButton()
		{
			OnClick?.Invoke(this);
		}
	}
}