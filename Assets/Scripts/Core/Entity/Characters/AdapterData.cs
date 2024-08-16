using UnityEngine;
using Utils;

namespace Core.Entity.Characters
{
    [CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(AdapterData), fileName = nameof(AdapterData))]
    public class AdapterData : ScriptableObject
    {
        [Range(0, 10)] public int SolverIteration;
    }
}