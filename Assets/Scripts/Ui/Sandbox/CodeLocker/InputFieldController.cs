using System.Text;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Sandbox.CodeLocker
{
	public class InputFieldController : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI[] _inputFields;
		[SerializeField] private Button _clearButton;
		[SerializeField] private NumberButton[] _buttons;

		private StringBuilder _stringBuilder;
		
		private void Start()
		{
			_stringBuilder = new StringBuilder();
			_clearButton.onClick.AddListener(ClearField);
			foreach (var button in _buttons)
			{
				button.Init();
				button.OnClick.Subscribe(EntryNumber).AddTo(this);
			}
			ClearField();
		}

		private void EntryNumber(int number)
		{
			if (_stringBuilder.Length >= _inputFields.Length)
				return;
			_stringBuilder.Append(number.ToString());
			UpdateUI();
		}

		private void UpdateUI()
		{
			for (int i = 0; i < _stringBuilder.Length; i++)
			{
				_inputFields[i].text = _stringBuilder[i].ToString();
			}
		}

		private void ClearField()
		{
			_stringBuilder.Clear();
			foreach (var textMeshProUGUI in _inputFields)
			{
				textMeshProUGUI.text = "";
			}
		}
	}
}
