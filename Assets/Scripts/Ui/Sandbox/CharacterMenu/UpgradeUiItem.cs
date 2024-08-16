using System;
using TMPro;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;
using uPools;

namespace Ui.Sandbox.CharacterMenu
{
	public class UpgradeUiItem : MonoBehaviour, IPoolCallbackReceiver
	{
		public TextMeshProUGUI UpgradeName;
		public ButtonWidget UpgradeButton;
		public Image[] Dots;
		
		public Color DotsDefaultColor;
		public Color DotsUpgradedColor;
		
		public Sprite EnabledUpgradeBtnIcon;
		public Sprite DisabledUpgradeBtnIcon;
		
		private IDisposable _disposable;

		public void OnRent()
		{
			gameObject.SetActive(true);
			UpgradeButton.Button.interactable = true;
		}
		
		public void OnReturn()
		{
			_disposable?.Dispose();
			MarkDotsUpgraded(0);
			gameObject.SetActive(false);
			UpgradeButton.Button.interactable = false;
		}

		public void MarkDotsUpgraded(int count)
		{
			for (int i = 0; i < Dots.Length; i++)
			{
				Dots[i].color = count > i ? DotsUpgradedColor : DotsDefaultColor;
			}
		}
		
		public void AttachDisposable(IDisposable disposable)
		{
			_disposable = disposable;
		}
		
		public void SetIsAvailable(bool isAvailable)
		{
			UpgradeButton.Button.interactable = isAvailable;
			UpgradeButton.Button.image.sprite = !isAvailable ? DisabledUpgradeBtnIcon : EnabledUpgradeBtnIcon;
		}
	}
}