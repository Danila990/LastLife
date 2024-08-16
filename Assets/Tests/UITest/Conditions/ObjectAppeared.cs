using UnityEngine;

namespace Tests.UITest.Conditions
{
    public class ObjectAppeared<T> : Condition<string> where T : Component
    {
        public override bool Satisfied()
        {
            var obj = Object.FindObjectOfType(typeof(T)) as T;
            return obj != null && obj.gameObject.activeInHierarchy;
        }
    }

    public class ObjectAppeared : Condition<string>
    {
        protected readonly string Path;
        public GameObject Obj;

        public ObjectAppeared(string path)
        {
            Path = path;
        }

        public override bool Satisfied()
        {
            Obj = GameObject.Find(Path);
            return Obj != null && Obj.activeInHierarchy;
        }

        public override string ToString()
        {
            return "ObjectAppeared(" + Path + ")";
        }
    }
}