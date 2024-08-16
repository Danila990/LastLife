using System;
using Core.Inventory.Items;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace Ui.Sandbox.InventoryUi
{
	public class InventoryPreviewItem : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private Image _icoImg;
		[SerializeField] private Image _bgImg;
		[SerializeField] private TextMeshProUGUI _ammoInfo;
		
		private float _defaultColorAlpha;
		private float _transparentColorAlpha;
		private MotionHandle _handle;
		public ItemContext CurrentInvItem { get; private set; }
		public event Action<InventoryPreviewItem> OnClick;
		
		public TextMeshProUGUI AmmoInfo => _ammoInfo;

		public void Init(ItemContext invItem)
		{
			_icoImg.sprite = invItem.InvIco;
			CurrentInvItem = invItem;
			
			_transparentColorAlpha = .2f;
			_defaultColorAlpha = _icoImg.color.a;
			
			var transparentColor = _icoImg.color;
			transparentColor.a = .2f;
			
			_icoImg.color = transparentColor;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			OnClick?.Invoke(this);
		}

		private void OnDisable()
		{
			_handle.IsActiveComplete();
		}

		public void Select()
		{
			if (!gameObject.activeInHierarchy)
			{
				var transparentColor = _icoImg.color;
				transparentColor.a = _defaultColorAlpha;
			
				_icoImg.color = transparentColor;
				return;
			}
			
			_handle.IsActiveCancel();

			_handle = LMotion
				.Create(_icoImg.color.a, _defaultColorAlpha, 0.25f)
				.BindToColorA(_icoImg);
		}

		public void Deselect()
		{
			if (!gameObject.activeInHierarchy)
			{
				var transparentColor = _icoImg.color;
				transparentColor.a = .2f;
			
				_icoImg.color = transparentColor;
				return;
			}

			_handle.IsActiveCancel();

			_handle = LMotion
				.Create(_icoImg.color.a, _transparentColorAlpha, 0.25f)
				.BindToColorA(_icoImg);
		}
	}
}