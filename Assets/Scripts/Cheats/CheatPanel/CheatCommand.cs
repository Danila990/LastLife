using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cheats.CheatPanel
{
	public class CheatCommand
	{
		public readonly Action<bool> Command;
		private readonly TextMeshProUGUI _text;
		private bool _state = false;
		private readonly Color _blackColor = Color.black;
		private readonly Color _whiteColor = Color.white;
		private readonly bool _toggle;
		public Button Button { get; }

		public CheatCommand(Action<bool> command, string description, Component button, bool toggle)
		{
			_toggle = toggle;
			Button = button.GetComponent<Button>();
			Command = command;	
			_text = button.GetComponentInChildren<TextMeshProUGUI>();
			Button.onClick.AddListener(Execute);
			_text.text = description;
			SetColor(_state);
		}

		private void Execute()
		{
			if(_toggle)
				_state = !_state;
			Command.Invoke(_state);
			SetColor(_state);
		}

		private void SetColor(bool state)
		{
			SetButtonColors(Button,state ? _whiteColor : _blackColor);
			_text.color = state ? _blackColor : _whiteColor;
		}
		
		private void SetButtonColors(Selectable button, Color color)
		{
			var colors = button.colors;
			colors.disabledColor = color;
			colors.highlightedColor = color;
			colors.normalColor = color;
			colors.pressedColor = _whiteColor;
			colors.selectedColor = color;
			button.colors = colors;
		}
	}
}