using UnityEngine;

namespace Tests.UITest.Conditions
{
    public class ObjectDisappeared<T> : Condition<string> where T : Component
    {
        public override bool Satisfied()
        {
            var obj = Object.FindObjectOfType(typeof(T)) as T;
            return obj == null || !obj.gameObject.activeInHierarchy;
        }
    }

    internal class ObjectDisappeared : ObjectAppeared
    {
        public ObjectDisappeared(string path) : base(path)
        {
        }

        public override bool Satisfied()
        {
            return !base.Satisfied();
        }

        public override string ToString()
        {
            return "ObjectDisappeared(" + Path + ")";
        }
    }
}