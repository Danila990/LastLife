using UnityEngine;
using UnityEngine.UI;

namespace Tests.UITest.Conditions
{
    public class AssertSliderValue : Condition<float>
    {
        public AssertSliderValue(string objectName, float val) : base(objectName, val)
        {
        }

        public override bool Satisfied()
        {
            return GetErrorMessage() == null;
        }

        private string GetErrorMessage()
        {
            var go = GameObject.Find(ObjectName);
            if (go == null) return "Slider object " + ObjectName + " does not exist";
            if (!go.activeInHierarchy) return "Slider object " + ObjectName + " is inactive";
            var slider = go.GetComponent<Slider>();
            if (slider == null) return "Slider object " + ObjectName + " has no Slider attached";
            if (!Mathf.Approximately(slider.value, Param))
                return "Slider " + ObjectName + "\n value expected: " + Param + ",\n actual: " + slider.value;
            return null;
        }

        public override string ToString()
        {
            return GetErrorMessage();
        }
    }
}