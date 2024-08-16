using Tests.UITest.Utils;
using UnityEngine;

namespace Tests.UITest.Conditions
{
    public class ObjectRotationZCondition : Condition<Interval>
    {
        public override bool Satisfied()
        {
            return GetErrorMessage() == null;
        }

        public ObjectRotationZCondition(string objectName, Interval param) : base(objectName, param)
        {
        }

        private string GetErrorMessage()
        {
            var go = GameObject.Find(ObjectName);
            if (go == null) return "Object " + ObjectName + " does not exist";
            if (!go.activeInHierarchy) return "Object " + ObjectName + " is inactive";
            var transform = go.GetComponent<Transform>();

            var eulerAngles = transform.eulerAngles;
            var insideInterval = eulerAngles.z >= Param.Origin && eulerAngles.z <= Param.End;
            
            if (!insideInterval) return "Object " + ObjectName + "\n z expected inside: " + Param + ",\n actual: " + eulerAngles.z;
            return null;
        }

        public override string ToString()
        {
            return GetErrorMessage();
        }

    }
}