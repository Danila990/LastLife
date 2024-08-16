using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Sandbox.CodeLocker
{
	public class NumberButton : MonoBehaviour
	{
		[SerializeField] private int _number;
		[SerializeField] private Button _button;

		private ReactiveCommand<int> _onClick;
		
		public IReactiveCommand<int> OnClick => _onClick;

		public void Init()
		{
			_onClick = new ReactiveCommand<int>();
			_onClick.AddTo(this);
			_button.onClick.AddListener(OnClickButton);
		}

		private void OnClickButton()
			=> _onClick?.Execute(_number);
	}
}
