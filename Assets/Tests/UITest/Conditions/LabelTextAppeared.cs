using UnityEngine;
using UnityEngine.UI;

namespace Tests.UITest.Conditions
{
    public class LabelTextAppeared : Condition<string>
    {
        public LabelTextAppeared(string objectName, string param) : base(objectName, param)
        {
        }

        public override bool Satisfied()
        {
            return GetErrorMessage() == null;
        }

        private string GetErrorMessage()
        {
            var go = GameObject.Find(ObjectName);
            if (go == null) return "Label object " + ObjectName + " does not exist";
            if (!go.activeInHierarchy) return "Label object " + ObjectName + " is inactive";
            var t = go.GetComponent<Text>();
            if (t == null) return "Label object " + ObjectName + " has no Text attached";
            if (t.text != Param) return "Label " + ObjectName + "\n text expected: " + Param + ",\n actual: " + t.text;
            return null;
        }

        public override string ToString()
        {
            return GetErrorMessage();
        }
    }
}