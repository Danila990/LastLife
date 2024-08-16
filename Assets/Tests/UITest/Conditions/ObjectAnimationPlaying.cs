using UnityEngine;

namespace Tests.UITest.Conditions
{
    public class ObjectAnimationPlaying : Condition<string>
    {
        public ObjectAnimationPlaying(string objectName, string param) : base(objectName, param)
        {
        }

        public override bool Satisfied()
        {
            var gameObject = GameObject.Find(ObjectName);
            return gameObject.GetComponent<Animation>().IsPlaying(Param);
        }
    }
}