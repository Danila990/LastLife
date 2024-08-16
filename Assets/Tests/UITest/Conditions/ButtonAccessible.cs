using UnityEngine;
using UnityEngine.UI;

namespace Tests.UITest.Conditions
{
    internal class ButtonAccessible : Condition<string>
    {
        private readonly GameObject _button;

        public ButtonAccessible(GameObject button)
        {
            _button = button;
        }

        public override bool Satisfied()
        {
            return GetAccessibilityMessage() == null;
        }

        public override string ToString()
        {
            return GetAccessibilityMessage() ?? "Button " + _button.name + " is accessible";
        }

        private string GetAccessibilityMessage()
        {
            if (_button == null)
                return "Button " + _button + " not found";
            if (_button.GetComponent<Button>() == null)
                return "GameObject " + _button + " does not have a Button component attached";
            return null;
        }
    }
}