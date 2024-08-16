using UnityEngine;
using Utils;

namespace Ui.Sandbox.WorldSpaceUI
{
    [CreateAssetMenu(menuName = SoNames.FACTORY_DATA+nameof(WorldSpaceUISO),fileName = nameof(WorldSpaceUISO))]
    public class WorldSpaceUISO : ScriptableObject, IWorldSpaceUISO
    {
        [SerializeField] private WorldSpaceUIData[] _uis;
        public WorldSpaceUIData[] UI => _uis;
    }
}