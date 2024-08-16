using System.Collections.Generic;
using Ui.Widget;
using UnityEngine;
using uPools;

namespace Ui.Sandbox.CharacterMenu
{
	public class ButtonOutlineWidgetPool : ObjectPoolBase<OutlineButtonWidget>
	{
		private readonly OutlineButtonWidget _prefab;
		private readonly Transform _parent;
		private Stack<OutlineButtonWidget> _pooled;

		public ButtonOutlineWidgetPool(OutlineButtonWidget prefab, Transform parent)
		{
			_prefab = prefab;
			_parent = parent;
			_pooled = new Stack<OutlineButtonWidget>();
		}

		protected override OutlineButtonWidget CreateInstance()
		{
			return Object.Instantiate(_prefab, _parent);
		}

		protected override void OnRent(OutlineButtonWidget instance)
		{
			_pooled.Push(instance);
		}

		protected override void OnReturn(OutlineButtonWidget instance)
		{
			instance.Button.interactable = true;
			instance.Button.onClick.RemoveAllListeners();
			instance.gameObject.SetActive(false);
			instance.DeselectOutline();
			
			if (instance.BGImg)
			{
				var color = instance.BGImg.color;
				color.a = 1f;
				instance.BGImg.color = color;
			}

			if (instance.IconImg)
			{
				var color = instance.IconImg.color;
				color.a = 1f;
				instance.IconImg.color = color;
			}
		}

		public void ReturnAll()
		{
			while (_pooled.Count > 0)
			{
				var instance = _pooled.Pop();
				Return(instance);
			}
		}
	}
}
