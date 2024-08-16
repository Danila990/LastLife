using UnityEngine;
using UnityEngine.UI;

namespace Tests.UITest.Conditions
{
    internal class ToggleValueAppeared : Condition<bool>
    {
        public ToggleValueAppeared(string objectName, bool val) : base(objectName, val)
        {
        }

        public override bool Satisfied()
        {
            return GetErrorMessage() == null;
        }

        private string GetErrorMessage()
        {
            var go = GameObject.Find(ObjectName);
            if (go == null) return "Toggle object " + ObjectName + " does not exist";
            if (!go.activeInHierarchy) return "Toggle object " + ObjectName + " is inactive";
            var t = go.GetComponent<Toggle>();
            if (t == null) return "Toggle object " + ObjectName + " has no Toggle attached";
            if (t.isOn != Param)
                return "Toggle " + ObjectName + "\n value expected: " + Param + ",\n actual: " + t.isOn;
            return null;
        }

        public override string ToString()
        {
            return GetErrorMessage();
        }
    }
}