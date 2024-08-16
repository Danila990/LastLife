using Sirenix.OdinInspector;
using UnityEngine;

namespace Ui.Sandbox.WorldSpaceUI
{
    public class WorldSpaceUI : MonoBehaviour
    {
        [BoxGroup("Runtime")] public Vector3 Offset;
        [BoxGroup("Runtime")] public Transform Target;
        [BoxGroup("Runtime")] public bool IsInactive;
        [BoxGroup("Runtime")] public string Key;
    }
}